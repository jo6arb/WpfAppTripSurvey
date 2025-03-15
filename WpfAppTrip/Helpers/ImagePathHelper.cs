using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Windows;

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
            
            // Проверяем наличие ресурса по ID варианта ответа
            string resourcePath = $"/Images/Quest{questionId}/{optionNumber}.jpg";
            if (IsResourceExists(resourcePath))
            {
                Debug.WriteLine($"Найден ресурс по ID варианта: {resourcePath}");
                return resourcePath;
            }
            
            // Проверяем другие расширения
            string[] extensions = { ".jpeg", ".jpg", ".png" };
            foreach (var ext in extensions)
            {
                string altPath = $"/Images/Quest{questionId}/{optionNumber}{ext}";
                if (IsResourceExists(altPath))
                {
                    Debug.WriteLine($"Найден ресурс с другим расширением: {altPath}");
                    return altPath;
                }
            }
            
            // Проверяем ресурсы по порядковому номеру (1, 2, 3) вместо ID варианта
            // Это нужно, так как в папках Quest1, Quest2 и т.д. файлы названы 1.jpg, 2.jpg, 3.jpg
            // а не по ID вариантов ответов
            int index = 0;
            
            // Определяем порядковый номер варианта ответа в зависимости от ID вопроса
            switch (questionId)
            {
                case 1: // Для первого вопроса
                    index = optionNumber; // ID совпадают с порядковыми номерами (1, 2, 3)
                    break;
                case 2: // Для второго вопроса
                    // ID вариантов: 4, 5, 6 -> порядковые номера: 1, 2, 3
                    index = optionNumber - 3;
                    break;
                case 3: // Для третьего вопроса
                    // ID вариантов: 7, 8, 9 -> порядковые номера: 1, 2, 3
                    index = optionNumber - 6;
                    break;
                case 4: // Для четвертого вопроса
                    // ID вариантов: 10, 11, 12 -> порядковые номера: 1, 2, 3
                    index = optionNumber - 9;
                    break;
                case 5: // Для пятого вопроса
                    // ID вариантов: 13, 14, 15 -> порядковые номера: 1, 2, 3
                    index = optionNumber - 12;
                    break;
                case 6: // Для шестого вопроса
                    // ID вариантов: 16, 17, 18 -> порядковые номера: 1, 2, 3
                    index = optionNumber - 15;
                    break;
                case 7: // Для седьмого вопроса
                    // ID вариантов: 19, 20, 21 -> порядковые номера: 1, 2, 3
                    index = optionNumber - 18;
                    break;
                case 8: // Для восьмого вопроса
                    // ID вариантов: 22, 23, 24 -> порядковые номера: 1, 2, 3
                    index = optionNumber - 21;
                    break;
                case 9: // Для девятого вопроса
                    // ID вариантов: 25, 26, 27 -> порядковые номера: 1, 2, 3
                    index = optionNumber - 24;
                    break;
                case 10: // Для десятого вопроса
                    // ID вариантов: 28, 29, 30 -> порядковые номера: 1, 2, 3
                    index = optionNumber - 27;
                    break;
                default:
                    // Для других вопросов пробуем вычислить порядковый номер
                    index = (optionNumber - 1) % 3 + 1;
                    break;
            }
            
            // Если индекс определен, проверяем ресурсы по порядковому номеру
            if (index > 0)
            {
                foreach (var ext in extensions)
                {
                    string indexPath = $"/Images/Quest{questionId}/{index}{ext}";
                    if (IsResourceExists(indexPath))
                    {
                        Debug.WriteLine($"Найден ресурс по порядковому номеру: {indexPath}");
                        return indexPath;
                    }
                }
            }
            
            // Проверяем файловую систему
            string[] folders = { 
                Path.Combine(ProjectDirectory, "Images", $"Quest{questionId}"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", $"Quest{questionId}")
            };
            
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
                    
                    // Проверяем файлы по ID варианта
                    foreach (var ext in extensions)
                    {
                        string filePath = Path.Combine(folder, $"{optionNumber}{ext}");
                        Debug.WriteLine($"Проверка файла по ID: {filePath}");
                        if (File.Exists(filePath))
                        {
                            Debug.WriteLine($"Файл найден по ID: {filePath}");
                            return filePath;
                        }
                    }
                    
                    // Проверяем файлы по порядковому номеру
                    if (index > 0)
                    {
                        foreach (var ext in extensions)
                        {
                            string filePath = Path.Combine(folder, $"{index}{ext}");
                            Debug.WriteLine($"Проверка файла по порядковому номеру: {filePath}");
                            if (File.Exists(filePath))
                            {
                                Debug.WriteLine($"Файл найден по порядковому номеру: {filePath}");
                                return filePath;
                            }
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"Папка не найдена: {folder}");
                }
            }
            
            Debug.WriteLine($"Изображение не найдено для вопроса {questionId}, вариант {optionNumber}");
            return null;
        }
        
        // Метод для проверки существования ресурса
        private static bool IsResourceExists(string resourcePath)
        {
            try
            {
                var packUri = $"pack://application:,,,{resourcePath}";
                var resourceInfo = Application.GetResourceStream(new Uri(packUri));
                bool exists = resourceInfo != null;
                Debug.WriteLine($"Проверка ресурса {resourcePath}: {exists}");
                return exists;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при проверке ресурса {resourcePath}: {ex.Message}");
                return false;
            }
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