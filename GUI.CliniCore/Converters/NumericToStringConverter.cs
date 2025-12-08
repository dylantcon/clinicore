using System.Globalization;

namespace GUI.CliniCore.Converters
{
    /// <summary>
    /// Converts nullable double to string for Entry binding.
    /// Returns empty string for null, otherwise formatted number.
    /// </summary>
    public class NullableDoubleToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double d)
                return d.ToString(culture);
            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string s && !string.IsNullOrWhiteSpace(s))
            {
                if (double.TryParse(s, NumberStyles.Any, culture, out var result))
                    return result;
            }
            return null;
        }
    }

    /// <summary>
    /// Converts int to string for Entry binding.
    /// Returns "0" for default, handles empty string as 0.
    /// </summary>
    public class IntToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int i)
                return i.ToString(culture);
            return "0";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string s && !string.IsNullOrWhiteSpace(s))
            {
                if (int.TryParse(s, NumberStyles.Any, culture, out var result))
                    return result;
            }
            return 0;
        }
    }

    /// <summary>
    /// Converts nullable int to string for Entry binding.
    /// Returns empty string for null.
    /// </summary>
    public class NullableIntToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int i)
                return i.ToString(culture);
            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string s && !string.IsNullOrWhiteSpace(s))
            {
                if (int.TryParse(s, NumberStyles.Any, culture, out var result))
                    return result;
            }
            return null;
        }
    }
}
