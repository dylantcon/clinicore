using System;

namespace API.CliniCore.Data.Entities.User
{
    /// <summary>
    /// EF Core entity for administrator persistence.
    /// Inherits common fields (Id, Username, CreatedAt, Name, Address, BirthDate) from ProfileEntityBase.
    /// </summary>
    public class AdministratorEntity : ProfileEntityBase
    {
        // Administrator-specific ProfileEntry
        public string Email { get; set; } = string.Empty;

        // Direct properties on AdministratorProfile
        public string Department { get; set; } = "Administration";

        // Permissions stored as JSON array of permission names
        public string PermissionsJson { get; set; } = "[]";
    }
}
