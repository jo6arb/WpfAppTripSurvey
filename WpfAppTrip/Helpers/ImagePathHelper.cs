using System;
using System.IO;

namespace WpfAppTrip.Helpers
{
    public static class ImagePathHelper
    {
        private static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        
        public static string GetAnswerImagePath(int questionId, int optionNumber)
        {
            return GetRelativePath($"Images/Answers/Question{questionId}/{optionNumber}.jpg");
        }

        private static string GetRelativePath(string relativePath)
        {
            // Преобразуем путь в относительный от исполняемого файла
            string fullPath = Path.Combine(BaseDirectory, relativePath);
            return fullPath;
        }

        public static string GetRelativePathForDb(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return null;
            
            // Преобразуем полный путь в относительный для хранения в БД
            return fullPath.Replace(BaseDirectory, "")
                         .TrimStart('\\', '/');
        }

        public static bool IsImageExists(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return false;
            
            string fullPath = Path.Combine(BaseDirectory, relativePath);
            return File.Exists(fullPath);
        }
    }
} 