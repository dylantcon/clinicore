using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.ProfileTemplates;
using Core.CliniCore.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Domain
{
    public class PhysicianProfile : AbstractUserProfile
    {
        public override UserRole Role => UserRole.Physician;

        // Convenience properties
        public string Name => GetValue<string>("name") ?? string.Empty;
        public string LicenseNumber => GetValue<string>("physician_license") ?? string.Empty;
        public DateTime GraduationDate => GetValue<DateTime>("physician_graduation");
        public List<MedicalSpecialization> Specializations =>
            GetValue<List<MedicalSpecialization>>("physician_specializations") ?? new();

        // Physician relationships
        public List<Guid> PatientIds { get; } = new();
        public List<Guid> AppointmentIds { get; } = new();
        public Dictionary<DayOfWeek, List<UnavailableTimeInterval>> StandardAvailability { get; } = new();

        protected override IProfileTemplate GetProfileTemplate()
            => new PhysicianProfileTemplate();

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Physician: Dr. {Name}");
            sb.AppendLine($"  ID: {Id:N}");
            sb.AppendLine($"  Username: {Username}");
            sb.AppendLine($"  License Number: {LicenseNumber}");
            sb.AppendLine($"  Graduation Date: {GraduationDate:yyyy-MM-dd}");

            if (Specializations != null && Specializations.Any())
            {
                sb.Append("  Specializations: ");
                var specializationNames = Specializations.Select(s => s.GetDisplayName());
                sb.AppendLine(string.Join(", ", specializationNames));
            }
            else
            {
                sb.AppendLine("  Specializations: None");
            }

            if (PatientIds.Any())
            {
                sb.AppendLine($"  Patients Under Care: {PatientIds.Count}");
            }

            if (AppointmentIds.Any())
            {
                sb.AppendLine($"  Scheduled Appointments: {AppointmentIds.Count}");
            }

            return sb.ToString();
        }
    }
}
