using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.IO;
using System.Diagnostics;

namespace WpfAppTrip.Converters
{
    /// <summary>
    /// Конвертер, который обрабатывает null значения для изображений
    /// </summary>
    public class NullImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return DependencyProperty.UnsetValue;

            try
            {
                string imagePath = value.ToString();
                
                // Проверяем, является ли путь ресурсом (начинается с /)
                if (imagePath.StartsWith("/"))
                {
                    try
                    {
                        // Создаем Pack URI для загрузки ресурса
                        string packUri = $"pack://application:,,,{imagePath}";
                        Debug.WriteLine($"Загрузка ресурса через Pack URI: {packUri}");
                        
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(packUri);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.CreateOptions = BitmapCreateOptions.None;
                        bitmap.DecodePixelWidth = 560;
                        bitmap.EndInit();
                        bitmap.Freeze(); // Для улучшения производительности
                        
                        return bitmap;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Ошибка при загрузке ресурса: {ex.Message}");
                        
                        // Пробуем альтернативный способ загрузки
                        try
                        {
                            // Убираем начальный слеш для загрузки как Content
                            string contentPath = imagePath.TrimStart('/');
                            Debug.WriteLine($"Пробуем загрузить как Content: {contentPath}");
                            
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(contentPath, UriKind.Relative);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.CreateOptions = BitmapCreateOptions.None;
                            bitmap.DecodePixelWidth = 560;
                            bitmap.EndInit();
                            bitmap.Freeze();
                            
                            return bitmap;
                        }
                        catch (Exception ex2)
                        {
                            Debug.WriteLine($"Ошибка при загрузке как Content: {ex2.Message}");
                        }
                        
                        return DependencyProperty.UnsetValue;
                    }
                }
                
                // Проверяем, существует ли файл
                if (File.Exists(imagePath))
                {
                    Debug.WriteLine($"Загрузка файла: {imagePath}");
                    
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.CreateOptions = BitmapCreateOptions.None;
                    bitmap.DecodePixelWidth = 560;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    
                    return bitmap;
                }
                
                Debug.WriteLine($"Файл не найден: {imagePath}");
                return DependencyProperty.UnsetValue;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка в NullImageConverter: {ex.Message}");
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
