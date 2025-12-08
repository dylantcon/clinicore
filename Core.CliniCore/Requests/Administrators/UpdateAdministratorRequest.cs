using System.Collections.Generic;

namespace Core.CliniCore.Requests.Administrators
{
    /// <summary>
    /// Request DTO for updating an existing administrator
    /// </summary>
    public class UpdateAdministratorRequest
    {
        // Common ProfileEntries (all optional for partial update)
        public string? Name { get; set; }
        public string? Address { get; set; }
        public DateTime? BirthDate { get; set; }

        // Administrator-specific ProfileEntry
        public string? Email { get; set; }

        // Direct properties
        public string? Department { get; set; }
        public List<string>? Permissions { get; set; }
    }
}
