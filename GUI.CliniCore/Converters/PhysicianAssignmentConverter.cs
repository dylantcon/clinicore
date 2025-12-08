using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.Service;
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
                return ProfileRegistry.GetProfileById(physicianId) is PhysicianProfile physician 
                    ? $"Dr. {physician.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty}" 
                    : "Assigned";
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
