using System;
using System.Collections.Generic;
using System.Linq;
using API.CliniCore.Common;
using Core.CliniCore.Api;
using Core.CliniCore.Mapping;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.DTOs.Physicians;
using Core.CliniCore.Requests.Physicians;
using Core.CliniCore.Service;
using Microsoft.AspNetCore.Mvc;
using Core.CliniCore.Domain.Users.Concrete;

namespace API.CliniCore.Controllers
{
    [ApiController]
    [Route(ApiRoutes.Physicians.BasePath)]
    public class PhysiciansController : ControllerBase
    {
        private readonly ProfileService _profileService;

        public PhysiciansController(ProfileService profileService)
        {
            _profileService = profileService;
        }

        /// <summary>
        /// Get all physicians
        /// </summary>
        [HttpGet]
        public ActionResult<IEnumerable<PhysicianDto>> GetAll()
        {
            var physicians = _profileService.GetAllPhysicians();
            return Ok(physicians.Select(p => p.ToDto()));
        }

        /// <summary>
        /// Get a physician by ID
        /// </summary>
        [HttpGet(ApiRoutes.Physicians.ById)]
        public ActionResult<PhysicianDto> GetById(Guid id)
        {
            var physician = _profileService.GetProfileById(id) as PhysicianProfile;
            if (physician == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Physician with ID {id} not found"));
            }

            return Ok(physician.ToDto());
        }

        /// <summary>
        /// Search physicians by name
        /// </summary>
        [HttpGet(ApiRoutes.Physicians.Search)]
        public ActionResult<IEnumerable<PhysicianDto>> Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest(ApiErrorResponse.FromMessage("Search query is required"));
            }

            var physicians = _profileService.GetAllPhysicians()
                .Where(p => (p.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty).Contains(q, StringComparison.OrdinalIgnoreCase));

            return Ok(physicians.Select(p => p.ToDto()));
        }

        /// <summary>
        /// Get physicians by specialization
        /// </summary>
        [HttpGet(ApiRoutes.Physicians.BySpecialization)]
        public ActionResult<IEnumerable<PhysicianDto>> GetBySpecialization(string specialization)
        {
            if (!Enum.TryParse<MedicalSpecialization>(specialization, true, out var spec))
            {
                return BadRequest(ApiErrorResponse.FromMessage($"Invalid specialization: {specialization}"));
            }

            var physicians = _profileService.GetAllPhysicians()
                .Where(p => (p.GetValue<List<MedicalSpecialization>>(PhysicianEntryType.Specializations.GetKey()) ?? new()).Contains(spec));
            return Ok(physicians.Select(p => p.ToDto()));
        }

        /// <summary>
        /// Create a new physician
        /// </summary>
        [HttpPost]
        public ActionResult<PhysicianDto> Create([FromBody] CreatePhysicianRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiErrorResponse.FromErrors("Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            // Parse specializations
            var specializations = MappingExtensions.ParseSpecializations(request.Specializations);
            if (!specializations.Any())
            {
                return BadRequest(ApiErrorResponse.FromMessage("At least one valid specialization is required"));
            }

            // Create the physician profile (use client-provided ID if available for credential linking)
            var physician = new PhysicianProfile
            {
                Id = request.Id ?? Guid.NewGuid(),
                Username = request.Username
            };

            // Set profile values (common)
            physician.SetValue(CommonEntryType.Name.GetKey(), request.Name);
            physician.SetValue(CommonEntryType.BirthDate.GetKey(), request.BirthDate);
            if (!string.IsNullOrEmpty(request.Address))
                physician.SetValue(CommonEntryType.Address.GetKey(), request.Address);

            // Set profile values (physician-specific)
            physician.SetValue(PhysicianEntryType.LicenseNumber.GetKey(), request.LicenseNumber);
            physician.SetValue(PhysicianEntryType.GraduationDate.GetKey(), request.GraduationDate);
            physician.SetValue(PhysicianEntryType.Specializations.GetKey(), specializations);

            // Add profile (credential creation is handled separately via /api/auth/register)
            _profileService.AddProfile(physician);

            return CreatedAtAction(nameof(GetById), new { id = physician.Id }, physician.ToDto());
        }

        /// <summary>
        /// Update an existing physician
        /// </summary>
        [HttpPut("{id}")]
        public ActionResult<PhysicianDto> Update(Guid id, [FromBody] UpdatePhysicianRequest request)
        {
            var physician = _profileService.GetProfileById(id) as PhysicianProfile;
            if (physician == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Physician with ID {id} not found"));
            }

            // Apply updates
            physician.ApplyUpdate(request);

            // Persist the changes
            _profileService.UpdateProfile(physician);

            return Ok(physician.ToDto());
        }

        /// <summary>
        /// Delete a physician
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var error = _profileService.RemoveProfile(id);

            if (error == "Profile not found")
                return NotFound(ApiErrorResponse.FromMessage($"Physician with ID {id} not found"));

            if (error != null)
                return BadRequest(ApiErrorResponse.FromMessage($"Cannot delete physician: {error}"));

            return NoContent();
        }

        /// <summary>
        /// Get list of valid specializations
        /// </summary>
        [HttpGet(ApiRoutes.Physicians.Specializations)]
        public ActionResult<IEnumerable<string>> GetSpecializations()
        {
            var specializations = Enum.GetNames<MedicalSpecialization>();
            return Ok(specializations);
        }

        /// <summary>
        /// Assign a patient to a physician
        /// </summary>
        [HttpPost(ApiRoutes.Physicians.AssignPatient)]
        public IActionResult AssignPatient(Guid id, Guid patientId)
        {
            var physician = _profileService.GetProfileById(id) as PhysicianProfile;
            if (physician == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Physician with ID {id} not found"));
            }

            var patient = _profileService.GetProfileById(patientId) as PatientProfile;
            if (patient == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Patient with ID {patientId} not found"));
            }

            _profileService.AssignPatientToPhysician(patientId, id);

            return Ok(new { Message = $"Patient assigned to physician successfully" });
        }

        /// <summary>
        /// Remove a patient from a physician
        /// </summary>
        [HttpDelete("{id}/patients/{patientId}")]
        public IActionResult RemovePatient(Guid id, Guid patientId)
        {
            var physician = _profileService.GetProfileById(id) as PhysicianProfile;
            if (physician == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Physician with ID {id} not found"));
            }

            if (!physician.PatientIds.Contains(patientId))
            {
                return NotFound(ApiErrorResponse.FromMessage(
                    $"Patient with ID {patientId} is not assigned to this physician"));
            }

            // Remove from physician's list
            physician.PatientIds.Remove(patientId);

            // Update patient's primary physician if it was this one
            var patient = _profileService.GetProfileById(patientId) as PatientProfile;
            if (patient != null && patient.PrimaryPhysicianId == id)
            {
                patient.PrimaryPhysicianId = null;
            }

            return NoContent();
        }
    }
}
