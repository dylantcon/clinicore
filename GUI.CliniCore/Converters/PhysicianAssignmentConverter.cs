using Core.CliniCore.Domain;
using System.Globalization;

namespace GUI.CliniCore.Converters
{
    /// <summary>
    /// Converts a PrimaryPhysicianId (Guid?) to a readable assignment status string
    /// </summary>
    public class PhysicianAssignmentConverter : IValueConverter
    {
        private readonly ProfileRegistry _profileRegistry = ProfileRegistry.Instance;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not Guid physicianId)
            {
                return "Unassigned";
            }

            var physician = _profileRegistry.GetProfileById(physicianId) as PhysicianProfile;
            return physician != null ? $"Dr. {physician.Name}" : "Assigned";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
