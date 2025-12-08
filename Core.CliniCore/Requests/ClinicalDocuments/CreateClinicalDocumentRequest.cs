using System;
using System.ComponentModel.DataAnnotations;

namespace Core.CliniCore.Requests.ClinicalDocuments
{
    /// <summary>
    /// Request DTO for creating a new clinical document
    /// </summary>
    public class CreateClinicalDocumentRequest
    {
        /// <summary>
        /// Optional client-generated ID for the document.
        /// If provided, the API will use this ID; otherwise, the API generates one.
        /// </summary>
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Patient ID is required")]
        public Guid PatientId { get; set; }

        [Required(ErrorMessage = "Physician ID is required")]
        public Guid PhysicianId { get; set; }

        [Required(ErrorMessage = "Appointment ID is required")]
        public Guid AppointmentId { get; set; }

        [Required(ErrorMessage = "Chief complaint is required")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Chief complaint must be between 1 and 500 characters")]
        public string ChiefComplaint { get; set; } = string.Empty;
    }
}
