using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace WpfAppTrip.Converters
{
    /// <summary>
    /// Конвертер для преобразования булевых значений в различные типы
    /// </summary>
    public class BooleanConverter : MarkupExtension, IValueConverter
    {
        /// <summary>
        /// Преобразует булево значение в указанный тип
        /// </summary>
        /// <param name="value">Булево значение</param>
        /// <param name="targetType">Целевой тип</param>
        /// <param name="parameter">Параметр конвертера в формате "TrueValue|FalseValue"</param>
        /// <param name="culture">Культура</param>
        /// <returns>Преобразованное значение</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string paramString)
            {
                string[] parts = paramString.Split('|');
                if (parts.Length == 2)
                {
                    string result = boolValue ? parts[0] : parts[1];
                    
                    // Проверяем, является ли результат ресурсом
                    if (result.StartsWith("{") && result.EndsWith("}"))
                    {
                        string resourceKey = result.Substring(1, result.Length - 2);
                        return Application.Current.Resources[resourceKey];
                    }
                    
                    // Если целевой тип - Visibility, преобразуем строку в Visibility
                    if (targetType == typeof(Visibility))
                    {
                        return result.Equals("Visible", StringComparison.OrdinalIgnoreCase) ? 
                            Visibility.Visible : Visibility.Collapsed;
                    }
                    
                    return result;
                }
            }
            
            return value;
        }

        /// <summary>
        /// Преобразует значение обратно в булево
        /// </summary>
        /// <param name="value">Значение</param>
        /// <param name="targetType">Целевой тип</param>
        /// <param name="parameter">Параметр конвертера</param>
        /// <param name="culture">Культура</param>
        /// <returns>Булево значение</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string paramString)
            {
                string[] parts = paramString.Split('|');
                if (parts.Length == 2 && value != null)
                {
                    string stringValue = value.ToString();
                    return stringValue.Equals(parts[0]);
                }
            }
            
            return false;
        }

        /// <summary>
        /// Возвращает экземпляр конвертера
        /// </summary>
        /// <param name="serviceProvider">Провайдер сервисов</param>
        /// <returns>Экземпляр конвертера</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
} 