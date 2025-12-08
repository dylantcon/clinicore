using API.CliniCore.Common;
using Core.CliniCore.Api;
using Core.CliniCore.Mapping;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.DTOs.Patients;
using Core.CliniCore.Requests.Patients;
using Core.CliniCore.Service;
using Microsoft.AspNetCore.Mvc;
using Core.CliniCore.Domain.Users.Concrete;

namespace API.CliniCore.Controllers
{
    [ApiController]
    [Route(ApiRoutes.Patients.BasePath)]
    public class PatientsController : ControllerBase
    {
        private readonly ProfileService _profileService;

        public PatientsController(ProfileService profileService)
        {
            _profileService = profileService;
        }

        /// <summary>
        /// Get all patients
        /// </summary>
        [HttpGet]
        public ActionResult<IEnumerable<PatientDto>> GetAll()
        {
            var patients = _profileService.GetAllPatients();
            var dtos = patients.Select(p =>
            {
                string? physicianName = null;
                if (p.PrimaryPhysicianId.HasValue)
                {
                    var physician = _profileService.GetProfileById(p.PrimaryPhysicianId.Value) as PhysicianProfile;
                    physicianName = physician?.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty;
                }
                return p.ToDto(physicianName);
            });
            return Ok(dtos);
        }

        /// <summary>
        /// Get a patient by ID
        /// </summary>
        [HttpGet(ApiRoutes.Patients.ById)]
        public ActionResult<PatientDto> GetById(Guid id)
        {
            var patient = _profileService.GetProfileById(id) as PatientProfile;
            if (patient == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Patient with ID {id} not found"));
            }

            string? physicianName = null;
            if (patient.PrimaryPhysicianId.HasValue)
            {
                var physician = _profileService.GetProfileById(patient.PrimaryPhysicianId.Value) as PhysicianProfile;
                physicianName = physician?.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty;
            }

            return Ok(patient.ToDto(physicianName));
        }

        /// <summary>
        /// Search patients by name
        /// </summary>
        [HttpGet(ApiRoutes.Patients.Search)]
        public ActionResult<IEnumerable<PatientDto>> Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest(ApiErrorResponse.FromMessage("Search query is required"));
            }

            var patients = _profileService.SearchByName(q).OfType<PatientProfile>();
            var dtos = patients.Select(p =>
            {
                string? physicianName = null;
                if (p.PrimaryPhysicianId.HasValue)
                {
                    var physician = _profileService.GetProfileById(p.PrimaryPhysicianId.Value) as PhysicianProfile;
                    physicianName = physician?.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty;
                }
                return p.ToDto(physicianName);
            });
            return Ok(dtos);
        }

        /// <summary>
        /// Create a new patient
        /// </summary>
        [HttpPost]
        public ActionResult<PatientDto> Create([FromBody] CreatePatientRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiErrorResponse.FromErrors("Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            // Validate physician exists if provided
            if (request.PrimaryPhysicianId.HasValue)
            {
                var physician = _profileService.GetProfileById(request.PrimaryPhysicianId.Value) as PhysicianProfile;
                if (physician == null)
                {
                    return BadRequest(ApiErrorResponse.FromMessage(
                        $"Physician with ID {request.PrimaryPhysicianId.Value} not found"));
                }
            }

            // Create the patient profile (use client-provided ID if available for credential linking)
            var patient = new PatientProfile
            {
                Id = request.Id ?? Guid.NewGuid(),
                Username = request.Username,
                PrimaryPhysicianId = request.PrimaryPhysicianId
            };

            // Set profile values
            patient.SetValue("name", request.Name);
            patient.SetValue("birthdate", request.BirthDate);
            patient.SetValue("patient_gender", MappingExtensions.ParseGender(request.Gender));

            if (!string.IsNullOrEmpty(request.Race))
                patient.SetValue("patient_race", request.Race);

            if (!string.IsNullOrEmpty(request.Address))
                patient.SetValue("address", request.Address);

            // Add profile (credential creation is handled separately via /api/auth/register)
            _profileService.AddProfile(patient);

            // Assign to physician if provided
            if (request.PrimaryPhysicianId.HasValue)
            {
                _profileService.AssignPatientToPhysician(patient.Id, request.PrimaryPhysicianId.Value);
            }

            string? physicianName = null;
            if (patient.PrimaryPhysicianId.HasValue)
            {
                var physician = _profileService.GetProfileById(patient.PrimaryPhysicianId.Value) as PhysicianProfile;
                physicianName = physician?.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty;
            }

            return CreatedAtAction(nameof(GetById), new { id = patient.Id }, patient.ToDto(physicianName));
        }

        /// <summary>
        /// Update an existing patient
        /// </summary>
        [HttpPut("{id}")]
        public ActionResult<PatientDto> Update(Guid id, [FromBody] UpdatePatientRequest request)
        {
            var patient = _profileService.GetProfileById(id) as PatientProfile;
            if (patient == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Patient with ID {id} not found"));
            }

            // Validate physician exists if being updated
            if (request.PrimaryPhysicianId.HasValue)
            {
                var physician = _profileService.GetProfileById(request.PrimaryPhysicianId.Value) as PhysicianProfile;
                if (physician == null)
                {
                    return BadRequest(ApiErrorResponse.FromMessage(
                        $"Physician with ID {request.PrimaryPhysicianId.Value} not found"));
                }
            }

            // Apply updates
            patient.ApplyUpdate(request);

            // Persist the changes
            _profileService.UpdateProfile(patient);

            // Handle physician assignment change
            if (request.PrimaryPhysicianId.HasValue &&
                request.PrimaryPhysicianId.Value != patient.PrimaryPhysicianId)
            {
                _profileService.AssignPatientToPhysician(patient.Id, request.PrimaryPhysicianId.Value);
            }

            string? physicianName = null;
            if (patient.PrimaryPhysicianId.HasValue)
            {
                var physician = _profileService.GetProfileById(patient.PrimaryPhysicianId.Value) as PhysicianProfile;
                physicianName = physician?.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty;
            }

            return Ok(patient.ToDto(physicianName));
        }

        /// <summary>
        /// Delete a patient
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var error = _profileService.RemoveProfile(id);

            if (error == "Profile not found")
                return NotFound(ApiErrorResponse.FromMessage($"Patient with ID {id} not found"));

            if (error != null)
                return BadRequest(ApiErrorResponse.FromMessage($"Cannot delete patient: {error}"));

            return NoContent();
        }

        /// <summary>
        /// Get patients assigned to a specific physician
        /// </summary>
        [HttpGet(ApiRoutes.Patients.ByPhysician)]
        public ActionResult<IEnumerable<PatientDto>> GetByPhysician(Guid physicianId)
        {
            var physician = _profileService.GetProfileById(physicianId) as PhysicianProfile;
            if (physician == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Physician with ID {physicianId} not found"));
            }

            var patients = physician.PatientIds
                .Select(pid => _profileService.GetProfileById(pid) as PatientProfile)
                .Where(p => p != null)
                .Select(p => p!.ToDto(physician.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty));

            return Ok(patients);
        }
    }
}
