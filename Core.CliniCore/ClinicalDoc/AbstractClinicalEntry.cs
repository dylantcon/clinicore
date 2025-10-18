// Core.CliniCore/ClinicalDoc/AbstractClinicalEntry.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.ClinicalDoc
{
    /// <summary>
    /// Base class for all clinical documentation entries
    /// </summary>
    public abstract class AbstractClinicalEntry
    {
        protected AbstractClinicalEntry(Guid authorId, string content)
        {
            Id = Guid.NewGuid();
            AuthorId = authorId;
            Content = content ?? string.Empty;
            CreatedAt = DateTime.Now;
            IsActive = true;
        }

        /// <summary>
        /// Unique identifier for this entry
        /// </summary>
        public Guid Id { get; protected set; }

        /// <summary>
        /// The physician who authored this entry
        /// </summary>
        public Guid AuthorId { get; protected set; }

        /// <summary>
        /// When this entry was created
        /// </summary>
        public DateTime CreatedAt { get; protected set; }

        /// <summary>
        /// When this entry was last modified
        /// </summary>
        public DateTime? ModifiedAt { get; set; }

        /// <summary>
        /// The actual content/text of the entry
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Whether this entry is active (not deleted/superseded)
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Optional ICD-10 or other coding
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// Severity or priority level
        /// </summary>
        public EntrySeverity Severity { get; set; } = EntrySeverity.Routine;

        /// <summary>
        /// Gets the type of this clinical entry
        /// </summary>
        public abstract ClinicalEntryType EntryType { get; }

        /// <summary>
        /// Validates that this entry is complete and valid
        /// </summary>
        public virtual bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Content) &&
                   AuthorId != Guid.Empty &&
                   IsActive;
        }

        /// <summary>
        /// Gets validation errors for this entry
        /// </summary>
        public virtual List<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Content))
                errors.Add($"{EntryType} entry must have content");

            if (AuthorId == Guid.Empty)
                errors.Add($"{EntryType} entry must have an author");

            return errors;
        }

        /// <summary>
        /// Creates a formatted display string for this entry
        /// </summary>
        public virtual string GetDisplayString()
        {
            var severityStr = Severity != EntrySeverity.Routine
                ? $"[{Severity}] "
                : "";

            var codeStr = !string.IsNullOrEmpty(Code)
                ? $" (Code: {Code})"
                : "";

            return $"{severityStr}{Content}{codeStr}";
        }

        /// <summary>
        /// Updates the entry and marks it as modified
        /// </summary>
        public virtual void Update(string newContent)
        {
            Content = newContent;
            ModifiedAt = DateTime.Now;
        }

        public override string ToString()
        {
            return $"{EntryType}: {GetDisplayString()} [{CreatedAt:yyyy-MM-dd HH:mm}]";
        }
    }

    /// <summary>
    /// Types of clinical entries
    /// </summary>
    public enum ClinicalEntryType
    {
        ChiefComplaint,
        Observation,
        Assessment,
        Diagnosis,
        Plan,
        Prescription,
        ProgressNote,
        Procedure,
        LabResult,
        VitalSigns
    }

    /// <summary>
    /// Severity levels for clinical entries
    /// </summary>
    public enum EntrySeverity
    {
        Routine,
        Moderate,
        Urgent,
        Critical,
        Emergency
    }
}
