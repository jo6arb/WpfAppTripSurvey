using System;
using System.IO;

namespace WpfAppTrip.Helpers
{
    public static class ImagePathHelper
    {
        private static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        
        /// <summary>
        /// Получает полный путь к изображению ответа по относительному пути из БД
        /// </summary>
        public static string GetFullImagePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return null;
            return Path.Combine(BaseDirectory, relativePath);
        }
        
        /// <summary>
        /// Получает относительный путь для хранения в БД
        /// </summary>
        public static string GetRelativePathForDb(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return null;
            
            // Преобразуем полный путь в относительный для хранения в БД
            return fullPath.Replace(BaseDirectory, "")
                         .TrimStart('\\', '/');
        }
        
        /// <summary>
        /// Проверяет существование изображения по относительному пути
        /// </summary>
        public static bool IsImageExists(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return false;
            
            string fullPath = Path.Combine(BaseDirectory, relativePath);
            return File.Exists(fullPath);
        }
        
        /// <summary>
        /// Получает URI для использования в XAML
        /// </summary>
        public static Uri GetImageUri(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return null;
            
            // Для использования в XAML (pack://application:,,,/)
            return new Uri($"pack://application:,,,/{relativePath}", UriKind.Absolute);
        }
    }
} 