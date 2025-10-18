using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.ProfileTemplates;

namespace Core.CliniCore.Domain
{
    public class AdministratorProfile : AbstractUserProfile
    {
        public override UserRole Role => UserRole.Administrator;

        // admin-specific properties
        public string Name => GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty;
        public string Department { get; set; } = "Administration";
        public List<Permission> GrantedPermissions { get; } = new();

        protected override IProfileTemplate GetProfileTemplate()
            => new AdministratorProfileTemplate();
    }
}
