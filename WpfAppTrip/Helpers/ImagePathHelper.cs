using System;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace WpfAppTrip.Helpers
{
    public static class ImagePathHelper
    {
        // Путь к папке проекта (не к bin/Debug)
        private static readonly string ProjectDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\.."));
        
        static ImagePathHelper()
        {
            // Выводим информацию о путях при инициализации
            Debug.WriteLine($"Путь к папке проекта: {ProjectDirectory}");
            Debug.WriteLine($"Путь к папке bin/Debug: {AppDomain.CurrentDomain.BaseDirectory}");
            
            // Проверяем наличие папки Images
            string imagesFolder = Path.Combine(ProjectDirectory, "Images");
            Debug.WriteLine($"Папка Images существует: {Directory.Exists(imagesFolder)}");
            
            if (Directory.Exists(imagesFolder))
            {
                // Выводим список подпапок
                var questFolders = Directory.GetDirectories(imagesFolder)
                    .Where(d => Path.GetFileName(d).StartsWith("Quest"))
                    .ToList();
                
                Debug.WriteLine($"Найдено {questFolders.Count} папок с вопросами:");
                foreach (var folder in questFolders)
                {
                    var files = Directory.GetFiles(folder);
                    Debug.WriteLine($"  {Path.GetFileName(folder)}: {files.Length} файлов");
                    foreach (var file in files)
                    {
                        Debug.WriteLine($"    {Path.GetFileName(file)}");
                    }
                }
            }
        }
        
        public static string GetAnswerImagePath(int questionId, int optionNumber)
        {
            // Проверяем различные варианты расширений
            string[] extensions = { ".jpeg", ".jpg", ".png" };
            string basePath = Path.Combine("Images", $"Quest{questionId}", $"{optionNumber}");
            
            foreach (var ext in extensions)
            {
                string fullPath = Path.Combine(ProjectDirectory, basePath + ext);
                if (File.Exists(fullPath))
                {
                    Debug.WriteLine($"Найден файл: {fullPath}");
                    return fullPath;
                }
            }
            
            Debug.WriteLine($"Файл не найден: {Path.Combine(ProjectDirectory, basePath + ".*")}");
            return null;
        }

        private static string GetRelativePath(string relativePath)
        {
            // Преобразуем путь в относительный от исполняемого файла
            string fullPath = Path.Combine(ProjectDirectory, relativePath);
            return fullPath;
        }

        public static string GetRelativePathForDb(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return null;
            
            // Преобразуем полный путь в относительный для хранения в БД
            return fullPath.Replace(ProjectDirectory, "")
                         .TrimStart('\\', '/');
        }

        public static bool IsImageExists(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            
            // Проверяем, является ли путь абсолютным
            if (Path.IsPathRooted(path))
            {
                bool exists = File.Exists(path);
                Debug.WriteLine($"Проверка абсолютного пути: {path}, существует: {exists}");
                return exists;
            }
            
            // Проверяем относительно папки проекта
            string projectPath = Path.Combine(ProjectDirectory, path);
            bool projectExists = File.Exists(projectPath);
            Debug.WriteLine($"Проверка пути относительно проекта: {projectPath}, существует: {projectExists}");
            
            if (projectExists) return true;
            
            // Проверяем относительно bin/Debug
            string binPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            bool binExists = File.Exists(binPath);
            Debug.WriteLine($"Проверка пути относительно bin/Debug: {binPath}, существует: {binExists}");
            
            return binExists;
        }
        
        public static string FixImagePath(string dbPath)
        {
            if (string.IsNullOrEmpty(dbPath)) return null;
            
            // Если путь уже содержит Images/Quest, используем его как есть
            if (dbPath.Contains("Images/Quest") || dbPath.Contains("Images\\Quest"))
            {
                return dbPath;
            }
            
            // Извлекаем номер вопроса и опции из пути
            try
            {
                string fileName = Path.GetFileName(dbPath);
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(dbPath);
                
                // Предполагаем, что имя файла - это номер опции
                if (int.TryParse(fileNameWithoutExt, out int optionNumber))
                {
                    // Извлекаем номер вопроса из пути (предполагаем, что он содержит Quest{номер})
                    string directory = Path.GetDirectoryName(dbPath);
                    if (directory != null)
                    {
                        string[] parts = directory.Split('\\', '/');
                        foreach (string part in parts)
                        {
                            if (part.StartsWith("Quest"))
                            {
                                string questPart = part.Substring(5); // Убираем "Quest"
                                if (int.TryParse(questPart, out int questionId))
                                {
                                    return GetAnswerImagePath(questionId, optionNumber);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при исправлении пути к изображению: {ex.Message}");
            }
            
            return dbPath;
        }

        public static string FindImageFile(int questionId, int optionNumber)
        {
            Debug.WriteLine($"Поиск изображения для вопроса {questionId}, вариант {optionNumber}");
            
            // Проверяем различные варианты путей и расширений
            string[] folders = { 
                Path.Combine(ProjectDirectory, "Images", $"Quest{questionId}"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", $"Quest{questionId}")
            };
            
            string[] extensions = { ".jpeg", ".jpg", ".png" };
            
            foreach (var folder in folders)
            {
                if (Directory.Exists(folder))
                {
                    Debug.WriteLine($"Папка найдена: {folder}");
                    
                    // Выводим список всех файлов в папке для диагностики
                    var allFiles = Directory.GetFiles(folder);
                    Debug.WriteLine($"Файлы в папке ({allFiles.Length}):");
                    foreach (var file in allFiles)
                    {
                        Debug.WriteLine($"  {Path.GetFileName(file)}");
                    }
                    
                    foreach (var ext in extensions)
                    {
                        string filePath = Path.Combine(folder, $"{optionNumber}{ext}");
                        Debug.WriteLine($"Проверка файла: {filePath}");
                        if (File.Exists(filePath))
                        {
                            Debug.WriteLine($"Файл найден: {filePath}");
                            return filePath;
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"Папка не найдена: {folder}");
                }
            }
            
            // Проверяем, есть ли файл по пути из базы данных
            string dbPath = $"Images/Quest{questionId}/{optionNumber}.jpg";
            string fullDbPath = Path.Combine(ProjectDirectory, dbPath);
            Debug.WriteLine($"Проверка пути из БД: {fullDbPath}");
            if (File.Exists(fullDbPath))
            {
                Debug.WriteLine($"Файл найден по пути из БД: {fullDbPath}");
                return fullDbPath;
            }
            
            // Проверяем все файлы в папке вопроса, которые начинаются с номера варианта
            foreach (var folder in folders)
            {
                if (Directory.Exists(folder))
                {
                    var matchingFiles = Directory.GetFiles(folder)
                        .Where(f => Path.GetFileNameWithoutExtension(f) == optionNumber.ToString())
                        .ToList();
                    
                    if (matchingFiles.Any())
                    {
                        Debug.WriteLine($"Найден файл по номеру варианта: {matchingFiles.First()}");
                        return matchingFiles.First();
                    }
                }
            }
            
            Debug.WriteLine($"Изображение не найдено для вопроса {questionId}, вариант {optionNumber}");
            return null;
        }
        
        // Метод для копирования изображений из папки проекта в bin/Debug
        public static void CopyImagesToOutput()
        {
            try
            {
                string sourceDir = Path.Combine(ProjectDirectory, "Images");
                string targetDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                
                if (!Directory.Exists(sourceDir))
                {
                    Debug.WriteLine("Папка с изображениями не найдена в проекте");
                    return;
                }
                
                // Создаем папку назначения, если её нет
                Directory.CreateDirectory(targetDir);
                
                // Копируем все подпапки и файлы
                foreach (string dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
                {
                    string newPath = dirPath.Replace(sourceDir, targetDir);
                    if (!Directory.Exists(newPath))
                    {
                        Directory.CreateDirectory(newPath);
                        Debug.WriteLine($"Создана папка: {newPath}");
                    }
                }
                
                foreach (string filePath in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
                {
                    string newPath = filePath.Replace(sourceDir, targetDir);
                    if (!File.Exists(newPath) || new FileInfo(filePath).LastWriteTime > new FileInfo(newPath).LastWriteTime)
                    {
                        File.Copy(filePath, newPath, true);
                        Debug.WriteLine($"Скопирован файл: {filePath} -> {newPath}");
                    }
                }
                
                Debug.WriteLine("Изображения успешно скопированы в выходную папку");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при копировании изображений: {ex.Message}");
                // Не выбрасываем исключение, чтобы не прерывать запуск приложения
            }
        }
    }
} 