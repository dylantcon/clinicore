using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.ProfileTemplates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Domain
{
    public class PatientProfile : AbstractUserProfile
    {
        public override UserRole Role => UserRole.Patient;

        // convenience properties for common access
        public string Name => GetValue<string>("name") ?? string.Empty;
        public DateTime BirthDate => GetValue<DateTime>("birthdate");
        public Gender Gender => GetValue<Gender>("patient_gender");
        public string Race => GetValue<string>("patient_race") ?? string.Empty;
        public string Address => GetValue<string>("address") ?? string.Empty;

        // patient relationships
        public List<Guid> AppointmentIds { get; } = new();
        public List<Guid> ClinicalDocumentIds { get; } = new();
        public Guid? PrimaryPhysicianId { get; set; }

        protected override IProfileTemplate GetProfileTemplate()
            => new PatientProfileTemplate();
    }
}
