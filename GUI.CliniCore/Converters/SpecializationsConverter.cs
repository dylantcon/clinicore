using System.Collections;
using System.Globalization;
using Core.CliniCore.Domain.Enumerations;

namespace GUI.CliniCore.Converters
{
    /// <summary>
    /// Converter that formats a list of MedicalSpecializations as a comma-separated string
    /// </summary>
    public class SpecializationsConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is IEnumerable<MedicalSpecialization> specializations)
            {
                var specList = specializations.ToList();
                if (specList.Any())
                {
                    return string.Join(", ", specList);
                }
                return "None";
            }

            return "None";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
