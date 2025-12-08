using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.ProfileTemplates;
using Core.CliniCore.Domain.Users;
using Core.CliniCore.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Domain.Users.Concrete
{
    public class PhysicianProfile : AbstractUserProfile
    {
        public override UserRole Role => UserRole.Physician;

        // Computed properties for XAML binding (delegate to ProfileEntry system)
        public string Name => GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty;
        public string LicenseNumber => GetValue<string>(PhysicianEntryType.LicenseNumber.GetKey()) ?? string.Empty;
        public DateTime GraduationDate => GetValue<DateTime>(PhysicianEntryType.GraduationDate.GetKey());
        public List<MedicalSpecialization> Specializations => GetValue<List<MedicalSpecialization>>(PhysicianEntryType.Specializations.GetKey()) ?? new();

        // Physician relationships
        public List<Guid> PatientIds { get; } = new();
        public List<Guid> AppointmentIds { get; } = new();
        public Dictionary<DayOfWeek, List<UnavailableTimeInterval>> StandardAvailability { get; } = new();

        protected override IProfileTemplate GetProfileTemplate()
            => new PhysicianProfileTemplate();

        public override string ToString()
        {
            var name = GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty;
            var licenseNumber = GetValue<string>(PhysicianEntryType.LicenseNumber.GetKey()) ?? string.Empty;
            var graduationDate = GetValue<DateTime>(PhysicianEntryType.GraduationDate.GetKey());
            var specializations = GetValue<List<MedicalSpecialization>>(PhysicianEntryType.Specializations.GetKey()) ?? new();

            var sb = new StringBuilder();
            sb.AppendLine($"Physician: Dr. {name}");
            sb.AppendLine($"  ID: {Id:N}");
            sb.AppendLine($"  Username: {Username}");
            sb.AppendLine($"  License Number: {licenseNumber}");
            sb.AppendLine($"  Graduation Date: {graduationDate:yyyy-MM-dd}");

            if (specializations != null && specializations.Any())
            {
                sb.Append("  Specializations: ");
                var specializationNames = specializations.Select(s => s.GetDisplayName());
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
