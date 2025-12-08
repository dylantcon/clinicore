using System;
using System.Collections.Generic;

namespace Core.CliniCore.DTOs.Administrators
{
    /// <summary>
    /// Response DTO representing an administrator profile.
    /// Inherits common fields (Id, Username, CreatedAt, Name, Address, BirthDate) from ProfileDtoBase.
    /// </summary>
    public class AdministratorDto : ProfileDtoBase
    {
        // Administrator-specific ProfileEntry
        public string Email { get; set; } = string.Empty;

        // Direct properties on AdministratorProfile
        public string Department { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
    }
}
