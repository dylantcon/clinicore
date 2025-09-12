using System;

namespace Core.CliniCore.Commands
{
    /// <summary>
    /// Defines standard parameter key names used across all commands
    /// This ensures consistency between command definitions and parameter parsing
    /// </summary>
    public static class CommandParameterKeys
    {
        // Authentication Parameters
        public const string Username = "username";
        public const string Password = "password";
        public const string OldPassword = "oldPassword";
        public const string NewPassword = "newPassword";
        public const string ConfirmPassword = "confirmPassword";

        // Profile Parameters
        public const string ProfileId = "profileId";
        public const string Name = "name";
        public const string Address = "address";
        public const string BirthDate = "birthdate";
        public const string Gender = "patient_gender";
        public const string Race = "patient_race";
        public const string LicenseNumber = "physician_license";
        public const string GraduationDate = "physician_graduation";
        public const string Specialization = "physician_specialization";
        public const string Email = "email";
        public const string Phone = "phone";

        // Patient/Physician IDs
        public const string PatientId = "patient_id";
        public const string PhysicianId = "physician_id";

        // Appointment Parameters
        public const string AppointmentId = "appointment_id";
        public const string AppointmentTime = "appointmentTime";
        public const string NewDateTime = "newDateTime";
        public const string Duration = "duration";
        public const string Notes = "notes";
        public const string DateTime = "dateTime";

        // Clinical Document Parameters
        public const string DocumentId = "document_id";
        public const string ChiefComplaint = "chief_complaint";
        public const string InitialObservation = "initial_observation";
        
        // Clinical Entry Parameters
        public const string Observation = "observation";
        public const string DiagnosisId = "diagnosis_id";
        public const string DiagnosisCode = "diagnosisCode";
        public const string DiagnosisDescription = "diagnosisDescription";
        public const string Medication = "medication";
        public const string Dosage = "dosage";
        public const string Frequency = "frequency";
        public const string Assessment = "assessment";
        public const string Plan = "plan";

        // Scheduling Parameters
        public const string DayOfWeek = "dayOfWeek";
        public const string StartTime = "startTime";
        public const string EndTime = "endTime";

        // Query Parameters
        public const string SearchTerm = "searchTerm";
        public const string Specializations = "specialization";

        // Report Parameters
        public const string StartDate = "startDate";
        public const string EndDate = "endDate";
        public const string ReportType = "reportType";

        // Facility Parameters
        public const string FacilityId = "facilityId";
        public const string FacilityName = "facilityName";
        public const string FacilityAddress = "facilityAddress";

        // Role Management Parameters
        public const string UserId = "userId";
        public const string UserRole = "userRole";
    }
}