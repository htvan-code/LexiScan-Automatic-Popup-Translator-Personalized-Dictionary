using System;
using System.Globalization;
using System.Windows.Data;

namespace LexiScan.App.Utilities
{
    public class StringMatchToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            return value.ToString().Equals(parameter.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue && parameter != null)
                return parameter.ToString();

            return Binding.DoNothing;
        }
    }
}