using System.ComponentModel.DataAnnotations;

namespace Core.CliniCore.Requests.ClinicalDocuments
{
    /// <summary>
    /// Request DTO for updating an existing clinical document
    /// </summary>
    public class UpdateClinicalDocumentRequest
    {
        /// <summary>
        /// Updated chief complaint. If null, the existing value is retained.
        /// </summary>
        [StringLength(500, ErrorMessage = "Chief complaint cannot exceed 500 characters")]
        public string? ChiefComplaint { get; set; }

        /// <summary>
        /// If true, marks the document as finalized/completed.
        /// Once completed, the document cannot be modified.
        /// </summary>
        public bool? IsCompleted { get; set; }
    }
}
