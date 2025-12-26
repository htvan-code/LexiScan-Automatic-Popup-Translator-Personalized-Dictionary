using System;
using System.Globalization;
using System.Windows.Data;

namespace LexiScan.App.Utilities
{
    // Converter để bind RadioButton với Enum
    public class EnumToBooleanConverter : IValueConverter
    {
        // Chuyển Enum sang bool
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || value == null)
                return false;

            string parameterString = parameter.ToString();
            string valueString = value.ToString();

            return parameterString.Equals(valueString, StringComparison.OrdinalIgnoreCase);
        }

        // Chuyển bool sang Enum
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || !(value is bool isChecked) || !isChecked)
                return Binding.DoNothing;

            string parameterString = parameter.ToString();
            return Enum.Parse(targetType, parameterString);
        }
    }
}