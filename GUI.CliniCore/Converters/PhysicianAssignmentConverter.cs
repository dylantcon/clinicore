using Core.CliniCore.Domain;
using Core.CliniCore.Services;
using System.Globalization;

namespace GUI.CliniCore.Converters
{
    /// <summary>
    /// Converts a PrimaryPhysicianId (Guid?) to a readable assignment status string.
    /// Uses service locator pattern since XAML converters can't use constructor injection.
    /// </summary>
    public class PhysicianAssignmentConverter : IValueConverter
    {
        private ProfileService? _profileRegistry;

        /// <summary>
        /// Gets ProfileService from MAUI DI container (lazy initialization)
        /// </summary>
        private ProfileService ProfileRegistry => _profileRegistry ??=
            Application.Current?.Handler?.MauiContext?.Services.GetRequiredService<ProfileService>()
            ?? throw new InvalidOperationException("ProfileService not available - ensure app is fully initialized");

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not Guid physicianId)
            {
                return "Unassigned";
            }

            try
            {
                var physician = ProfileRegistry.GetProfileById(physicianId) as PhysicianProfile;
                return physician != null ? $"Dr. {physician.Name}" : "Assigned";
            }
            catch
            {
                return "Assigned"; // Fallback if service not ready
            }
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
