using System.Globalization;

namespace GUI.CliniCore.Converters
{
    /// <summary>
    /// Converter that returns true if string value is not null and not empty, false otherwise
    /// </summary>
    public class IsStringNotNullOrEmptyConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return !string.IsNullOrEmpty(value as string);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
