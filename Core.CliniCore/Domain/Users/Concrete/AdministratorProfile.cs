using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.ProfileTemplates;
using Core.CliniCore.Domain.Users;

namespace Core.CliniCore.Domain.Users.Concrete
{
    public class AdministratorProfile : AbstractUserProfile
    {
        public override UserRole Role => UserRole.Administrator;

        // Computed property for XAML binding (delegates to ProfileEntry system)
        public string Name => GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty;

        // Admin-specific properties
        public string Department { get; set; } = "Administration";
        public List<Permission> GrantedPermissions { get; } = new();

        protected override IProfileTemplate GetProfileTemplate()
            => new AdministratorProfileTemplate();
    }
}
