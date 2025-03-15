using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace WpfAppTrip.Converters
{
    /// <summary>
    /// Альтернативный конвертер для изображений, который пробует различные способы загрузки
    /// </summary>
    public class ImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return DependencyProperty.UnsetValue;

            string imagePath = value.ToString();
            Debug.WriteLine($"ImageSourceConverter: Загрузка изображения из {imagePath}");

            try
            {
                // Проверяем, является ли путь ресурсом (начинается с /)
                if (imagePath.StartsWith("/"))
                {
                    // Пробуем загрузить как Pack URI
                    try
                    {
                        string packUri = $"pack://application:,,,{imagePath}";
                        Debug.WriteLine($"Пробуем загрузить через Pack URI: {packUri}");
                        
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(packUri);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.CreateOptions = BitmapCreateOptions.None;
                        bitmap.DecodePixelWidth = 560;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        
                        return bitmap;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Ошибка при загрузке через Pack URI: {ex.Message}");
                        
                        // Пробуем загрузить как относительный путь
                        try
                        {
                            string relativePath = imagePath.TrimStart('/');
                            Debug.WriteLine($"Пробуем загрузить как относительный путь: {relativePath}");
                            
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(relativePath, UriKind.Relative);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.CreateOptions = BitmapCreateOptions.None;
                            bitmap.DecodePixelWidth = 560;
                            bitmap.EndInit();
                            bitmap.Freeze();
                            
                            return bitmap;
                        }
                        catch (Exception ex2)
                        {
                            Debug.WriteLine($"Ошибка при загрузке как относительный путь: {ex2.Message}");
                        }
                    }
                }
                
                // Проверяем, существует ли файл
                if (File.Exists(imagePath))
                {
                    Debug.WriteLine($"Загрузка файла напрямую: {imagePath}");
                    
                    try
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        bitmap.DecodePixelWidth = 560;
                        
                        // Загружаем файл в память
                        using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                        {
                            var memoryStream = new MemoryStream();
                            stream.CopyTo(memoryStream);
                            memoryStream.Position = 0;
                            
                            bitmap.StreamSource = memoryStream;
                            bitmap.EndInit();
                            bitmap.Freeze();
                            
                            return bitmap;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Ошибка при загрузке файла напрямую: {ex.Message}");
                    }
                }
                
                // Если все способы не сработали, пробуем загрузить через абсолютный URI
                try
                {
                    Debug.WriteLine($"Пробуем загрузить через абсолютный URI: {imagePath}");
                    
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
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка при загрузке через абсолютный URI: {ex.Message}");
                }
                
                Debug.WriteLine($"Не удалось загрузить изображение: {imagePath}");
                return DependencyProperty.UnsetValue;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Общая ошибка в ImageSourceConverter: {ex.Message}");
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 