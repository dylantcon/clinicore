using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.ProfileTemplates;
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

        // public Dictionary<DayOfWeek, List<ITimeInterval>> StandardAvailability { get; } = new();

        protected override IProfileTemplate GetProfileTemplate()
            => new PhysicianProfileTemplate();
    }
}
