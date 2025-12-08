using System;

namespace Core.CliniCore.DTOs
{
    /// <summary>
    /// Base class for all profile DTOs.
    /// Contains common fields from AbstractProfileTemplate/CommonEntryType.
    /// </summary>
    public abstract class ProfileDtoBase
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
