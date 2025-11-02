using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace BulkIn.Desktop.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string colorParam)
            {
                var colors = colorParam.Split(':');
                if (colors.Length == 2)
                {
                    var trueColor = colors[0];
                    var falseColor = colors[1];
                    var selectedColor = boolValue ? trueColor : falseColor;
                    
                    return Brush.Parse(selectedColor);
                }
            }
            
            return Brushes.Transparent;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
