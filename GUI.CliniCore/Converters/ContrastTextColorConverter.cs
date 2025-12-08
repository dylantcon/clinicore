using System.Globalization;

namespace GUI.CliniCore.Converters
{
    /// <summary>
    /// Converts a background color to a contrasting text color (black or white)
    /// based on relative luminance calculation (ITU-R BT.709).
    /// </summary>
    public class ContrastTextColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var backgroundColor = value switch
            {
                Color color => color,
                string hex => Color.FromArgb(hex),
                SolidColorBrush brush => brush.Color,
                _ => Colors.White // Default assumption: light background
            };

            return GetContrastingTextColor(backgroundColor);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();

        /// <summary>
        /// Calculates contrasting text color based on background luminance.
        /// Uses ITU-R BT.709 formula for relative luminance.
        /// </summary>
        public static Color GetContrastingTextColor(Color background)
        {
            // ITU-R BT.709 relative luminance formula
            var luminance = 0.299 * background.Red + 0.587 * background.Green + 0.114 * background.Blue;

            // Return black text for light backgrounds, white for dark
            return luminance > 0.5 ? Colors.Black : Colors.White;
        }
    }
}
