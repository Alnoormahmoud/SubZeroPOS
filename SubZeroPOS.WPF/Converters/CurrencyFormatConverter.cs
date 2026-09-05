using System;
using System.Globalization;
using System.Windows.Data;
using SubZeroPOS.WPF.Session;

namespace SubZeroPOS.WPF.Converters
{
    public class CurrencyFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal d)
                return $"{d:#,##0.000} {CurrencyHolder.Symbol}";
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}