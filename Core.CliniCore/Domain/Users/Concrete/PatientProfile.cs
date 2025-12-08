using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.ProfileTemplates;
using Core.CliniCore.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Domain.Users.Concrete
{
    public class PatientProfile : AbstractUserProfile, IIdentifiable
    {
        public override UserRole Role => UserRole.Patient;

        // Computed properties for XAML binding (delegate to ProfileEntry system)
        public string Name => GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty;
        public string Address => GetValue<string>(CommonEntryType.Address.GetKey()) ?? string.Empty;
        public DateTime BirthDate => GetValue<DateTime>(CommonEntryType.BirthDate.GetKey());
        public Gender Gender => GetValue<Gender>(PatientEntryType.Gender.GetKey());
        public string Race => GetValue<string>(PatientEntryType.Race.GetKey()) ?? string.Empty;

        // Patient relationships
        public List<Guid> AppointmentIds { get; } = new();
        public List<Guid> ClinicalDocumentIds { get; } = new();
        public Guid? PrimaryPhysicianId { get; set; }

        protected override IProfileTemplate GetProfileTemplate()
            => new PatientProfileTemplate();

        public override string ToString()
        {
            var birthDate = GetValue<DateTime>(CommonEntryType.BirthDate.GetKey());
            var age = DateTime.Now.Year - birthDate.Year;
            if (DateTime.Now.DayOfYear < birthDate.DayOfYear) age--;

            var name = GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty;
            var gender = GetValue<Gender>(PatientEntryType.Gender.GetKey());
            var race = GetValue<string>(PatientEntryType.Race.GetKey()) ?? string.Empty;
            var address = GetValue<string>(CommonEntryType.Address.GetKey()) ?? string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine($"Patient: {name} (Age: {age})");
            sb.AppendLine($"  ID: {Id:N}");
            sb.AppendLine($"  Username: {Username}");
            sb.AppendLine($"  Gender: {gender}");
            sb.AppendLine($"  Race: {race}");
            sb.AppendLine($"  Birth Date: {birthDate:yyyy-MM-dd}");
            sb.AppendLine($"  Address: {address}");

            if (PrimaryPhysicianId.HasValue)
            {
                sb.AppendLine($"  Primary Physician ID: {PrimaryPhysicianId.Value:N}");
            }
            else
            {
                sb.AppendLine("  Primary Physician: None");
            }

            if (AppointmentIds.Any())
            {
                sb.AppendLine($"  Appointments: {AppointmentIds.Count}");
            }

            if (ClinicalDocumentIds.Any())
            {
                sb.AppendLine($"  Clinical Documents: {ClinicalDocumentIds.Count}");
            }

            return sb.ToString();
        }
    }
}
