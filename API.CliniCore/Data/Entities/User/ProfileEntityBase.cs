using System;

namespace API.CliniCore.Data.Entities.User
{
    /// <summary>
    /// Base class for all profile entities.
    /// Contains common fields from AbstractProfileTemplate/CommonEntryType.
    /// </summary>
    public abstract class ProfileEntityBase
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Common ProfileEntry fields (from CommonEntryType)
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
    }
}
