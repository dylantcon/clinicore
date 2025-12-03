using Core.CliniCore.Services;
using System.Linq;

namespace Core.CliniCore.Domain.Validation.Validators
{
    /// <summary>
    /// Validates that a username is unique across all profiles in the system
    /// </summary>
    public class UsernameUniquenessValidator : AbstractValidator<string>
    {
        private readonly ProfileService _profileRegistry;

        public UsernameUniquenessValidator(ProfileService profileRegistry, string? customErrorMessage = null)
            : base(customErrorMessage ?? "Username is already taken")
        {
            _profileRegistry = profileRegistry ?? throw new System.ArgumentNullException(nameof(profileRegistry));
        }

        public override bool IsValid(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true; // Let RequiredValidator handle null/empty validation

            // Check all profile types for username conflicts
            var allProfiles = new System.Collections.Generic.List<IUserProfile>();
            allProfiles.AddRange(_profileRegistry.GetAllAdministrators());
            allProfiles.AddRange(_profileRegistry.GetAllPhysicians());
            allProfiles.AddRange(_profileRegistry.GetAllPatients());

            // Check if any existing profile has this username
            return !allProfiles.Any(profile => 
                string.Equals(profile.Username, value, System.StringComparison.OrdinalIgnoreCase));
        }
    }
}