using System;
using System.Collections.Generic;
using System.Linq;
using API.CliniCore.Common;
using Core.CliniCore.Api;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.DTOs.Appointments;
using Core.CliniCore.Mapping;
using Core.CliniCore.Requests.Appointments;
using Core.CliniCore.Scheduling;
using Core.CliniCore.Service;
using Microsoft.AspNetCore.Mvc;

namespace API.CliniCore.Controllers
{
    [ApiController]
    [Route(ApiRoutes.Appointments.BasePath)]
    public class AppointmentsController : ControllerBase
    {
        private readonly SchedulerService _schedulerService;
        private readonly ProfileService _profileService;

        public AppointmentsController(SchedulerService schedulerService, ProfileService profileService)
        {
            _schedulerService = schedulerService;
            _profileService = profileService;
        }

        /// <summary>
        /// Get all appointments
        /// </summary>
        [HttpGet]
        public ActionResult<IEnumerable<AppointmentDto>> GetAll(
            [FromQuery] string? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var appointments = _schedulerService.GetAllAppointments();

            // Filter by status
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<AppointmentStatus>(status, true, out var statusEnum))
            {
                appointments = appointments.Where(a => a.Status == statusEnum);
            }

            // Filter by date range
            if (fromDate.HasValue)
            {
                appointments = appointments.Where(a => a.Start >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                appointments = appointments.Where(a => a.Start <= toDate.Value);
            }

            var dtos = appointments.Select(a => a.ToDto(
                GetPatientName(a.PatientId),
                GetPhysicianName(a.PhysicianId)));

            return Ok(dtos);
        }

        /// <summary>
        /// Get an appointment by ID
        /// </summary>
        [HttpGet(ApiRoutes.Appointments.ById)]
        public ActionResult<AppointmentDto> GetById(Guid id)
        {
            var appointment = _schedulerService.FindAppointmentById(id);
            if (appointment == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Appointment with ID {id} not found"));
            }

            return Ok(appointment.ToDto(
                GetPatientName(appointment.PatientId),
                GetPhysicianName(appointment.PhysicianId)));
        }

        /// <summary>
        /// Get appointments for a patient
        /// </summary>
        [HttpGet(ApiRoutes.Appointments.ByPatient)]
        public ActionResult<IEnumerable<AppointmentDto>> GetByPatient(Guid patientId,
            [FromQuery] string? status = null)
        {
            var patient = _profileService.GetProfileById(patientId) as PatientProfile;
            if (patient == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Patient with ID {patientId} not found"));
            }

            var appointments = _schedulerService.GetPatientAppointments(patientId);

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<AppointmentStatus>(status, true, out var statusEnum))
            {
                appointments = appointments.Where(a => a.Status == statusEnum);
            }

            var dtos = appointments.Select(a => a.ToDto(
                patient.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty,
                GetPhysicianName(a.PhysicianId)));

            return Ok(dtos);
        }

        /// <summary>
        /// Get appointments for a physician
        /// </summary>
        [HttpGet(ApiRoutes.Appointments.ByPhysician)]
        public ActionResult<IEnumerable<AppointmentDto>> GetByPhysician(Guid physicianId,
            [FromQuery] DateTime? date = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var physician = _profileService.GetProfileById(physicianId) as PhysicianProfile;
            if (physician == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Physician with ID {physicianId} not found"));
            }

            IEnumerable<AppointmentTimeInterval> appointments;

            if (date.HasValue)
            {
                appointments = _schedulerService.GetDailySchedule(physicianId, date.Value);
            }
            else if (fromDate.HasValue && toDate.HasValue)
            {
                appointments = _schedulerService.GetScheduleInRange(physicianId, fromDate.Value, toDate.Value);
            }
            else
            {
                // Default to next 30 days
                appointments = _schedulerService.GetScheduleInRange(
                    physicianId,
                    DateTime.Today,
                    DateTime.Today.AddDays(30));
            }

            var dtos = appointments.Select(a => a.ToDto(
                GetPatientName(a.PatientId),
                physician.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty));

            return Ok(dtos);
        }

        /// <summary>
        /// Create a new appointment
        /// </summary>
        [HttpPost]
        public ActionResult<AppointmentDto> Create([FromBody] CreateAppointmentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiErrorResponse.FromErrors("Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            // Validate patient exists
            PatientProfile? patient = _profileService.GetProfileById(request.PatientId) as PatientProfile;
            if (patient == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Patient with ID {request.PatientId} not found"));
            }

            // Validate physician exists
            PhysicianProfile? physician = _profileService.GetProfileById(request.PhysicianId) as PhysicianProfile;
            if (physician == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Physician with ID {request.PhysicianId} not found"));
            }

            // Validate time
            if (request.End <= request.Start)
            {
                return BadRequest(ApiErrorResponse.FromMessage("End time must be after start time"));
            }

            if (request.Start < DateTime.Now)
            {
                return BadRequest(ApiErrorResponse.FromMessage("Cannot schedule appointments in the past"));
            }

            // Create appointment
            var appointment = new AppointmentTimeInterval(
                request.Start,
                request.End,
                request.PatientId,
                request.PhysicianId,
                request.ReasonForVisit ?? string.Empty);

            if (!string.IsNullOrEmpty(request.Notes))
            {
                appointment.Notes = request.Notes;
            }

            if (!string.IsNullOrEmpty(request.ReasonForVisit))
            {
                appointment.ReasonForVisit = request.ReasonForVisit;
            }

            // Set room number if provided
            if (request.RoomNumber.HasValue)
            {
                appointment.RoomNumber = request.RoomNumber.Value;
            }

            // Schedule the appointment
            var result = _schedulerService.ScheduleAppointment(appointment);

            if (!result.Success)
            {
                var errorResponse = ApiErrorResponse.FromMessage(result.Message);
                if (result.Conflicts.Any())
                {
                    errorResponse.Errors = [.. result.Conflicts.Select(c => c.ToString() ?? String.Empty)];
                }
                return Conflict(errorResponse);
            }

            // Add to patient's and physician's appointment lists
            patient.AppointmentIds.Add(appointment.Id);
            physician.AppointmentIds.Add(appointment.Id);

            return CreatedAtAction(nameof(GetById), new { id = appointment.Id },
                appointment.ToDto(patient.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty, physician.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty));
        }

        /// <summary>
        /// Update an existing appointment
        /// </summary>
        [HttpPut("{id}")]
        public ActionResult<AppointmentDto> Update(Guid id, [FromBody] UpdateAppointmentRequest request)
        {
            var appointment = _schedulerService.FindAppointmentById(id);
            if (appointment == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Appointment with ID {id} not found"));
            }

            // Use scheduler service update which handles conflict detection
            var result = _schedulerService.UpdateAppointment(
                id,
                request.ReasonForVisit,
                request.Notes,
                request.Start.HasValue && request.End.HasValue
                    ? (int?)(request.End.Value - request.Start.Value).TotalMinutes
                    : null,
                request.Start,
                request.RoomNumber);

            if (!result.Success)
            {
                var errorResponse = ApiErrorResponse.FromMessage(result.Message);
                if (result.Conflicts.Any())
                {
                    errorResponse.Errors = result.Conflicts.Select(c => c.ToString() ?? String.Empty).ToList();
                }
                return Conflict(errorResponse);
            }

            // Handle status change separately - must be persisted via scheduler service
            if (!string.IsNullOrEmpty(request.Status) &&
                Enum.TryParse<AppointmentStatus>(request.Status, true, out var status) &&
                status != appointment.Status)
            {
                if (status == AppointmentStatus.Cancelled)
                {
                    // Use the scheduler service to properly persist the cancellation
                    _schedulerService.CancelAppointment(
                        appointment.PhysicianId,
                        id,
                        request.CancellationReason ?? string.Empty);

                    // Refresh the appointment to get the updated status
                    appointment = _schedulerService.FindAppointmentById(id)!;
                }
                else
                {
                    // For other status changes, update and persist via scheduler service
                    _schedulerService.UpdateAppointmentStatus(id, status);
                    appointment = _schedulerService.FindAppointmentById(id)!;
                }
            }

            return Ok(appointment.ToDto(
                GetPatientName(appointment.PatientId),
                GetPhysicianName(appointment.PhysicianId)));
        }

        /// <summary>
        /// Cancel an appointment
        /// </summary>
        [HttpPost(ApiRoutes.Appointments.Cancel)]
        public ActionResult<AppointmentDto> Cancel(Guid id, [FromBody] CancelAppointmentRequest? request = null)
        {
            var appointment = _schedulerService.FindAppointmentById(id);
            if (appointment == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Appointment with ID {id} not found"));
            }

            if (!appointment.CanCancel())
            {
                return BadRequest(ApiErrorResponse.FromMessage(
                    $"Appointment cannot be cancelled. Current status: {appointment.Status}"));
            }

            var success = _schedulerService.CancelAppointment(
                appointment.PhysicianId,
                id,
                request?.Reason ?? string.Empty);

            if (!success)
            {
                return BadRequest(ApiErrorResponse.FromMessage("Failed to cancel appointment"));
            }

            return Ok(appointment.ToDto(
                GetPatientName(appointment.PatientId),
                GetPhysicianName(appointment.PhysicianId)));
        }

        /// <summary>
        /// Delete an appointment
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var appointment = _schedulerService.FindAppointmentById(id);
            if (appointment == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Appointment with ID {id} not found"));
            }

            // Remove from patient and physician lists
            var patient = _profileService.GetProfileById(appointment.PatientId) as PatientProfile;
            patient?.AppointmentIds.Remove(id);

            var physician = _profileService.GetProfileById(appointment.PhysicianId) as PhysicianProfile;
            physician?.AppointmentIds.Remove(id);

            var success = _schedulerService.DeleteAppointment(appointment.PhysicianId, id);
            if (!success)
            {
                return BadRequest(ApiErrorResponse.FromMessage("Failed to delete appointment"));
            }

            return NoContent();
        }

        /// <summary>
        /// Get available time slots for a physician
        /// </summary>
        [HttpGet(ApiRoutes.Appointments.AvailableSlots)]
        public ActionResult<IEnumerable<object>> GetAvailableSlots(Guid physicianId,
            [FromQuery] int durationMinutes = 30,
            [FromQuery] DateTime? afterDate = null,
            [FromQuery] int maxSlots = 10)
        {
            var physician = _profileService.GetProfileById(physicianId) as PhysicianProfile;
            if (physician == null)
            {
                return NotFound(ApiErrorResponse.FromMessage($"Physician with ID {physicianId} not found"));
            }

            var slots = new List<object>();
            var duration = TimeSpan.FromMinutes(durationMinutes);
            var searchStart = afterDate ?? DateTime.Now;

            for (int i = 0; i < maxSlots; i++)
            {
                var slot = _schedulerService.FindNextAvailableSlot(physicianId, duration, searchStart);
                if (slot == null) break;

                slots.Add(new
                {
                    Start = slot.Start,
                    End = slot.End,
                    DurationMinutes = (slot.End - slot.Start).TotalMinutes,
                    IsOptimal = slot.IsOptimal
                });

                searchStart = slot.End;
            }

            return Ok(slots);
        }

        /// <summary>
        /// Get appointment status values
        /// </summary>
        [HttpGet(ApiRoutes.Appointments.Statuses)]
        public ActionResult<IEnumerable<string>> GetStatuses()
        {
            return Ok(Enum.GetNames<AppointmentStatus>());
        }

        #region Helper Methods

        private string? GetPatientName(Guid patientId)
        {
            var patient = _profileService.GetProfileById(patientId) as PatientProfile;
            return patient?.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty;
        }

        private string? GetPhysicianName(Guid physicianId)
        {
            var physician = _profileService.GetProfileById(physicianId) as PhysicianProfile;
            return physician?.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty;
        }

        #endregion
    }

    /// <summary>
    /// Request for cancelling an appointment
    /// </summary>
    public class CancelAppointmentRequest
    {
        public string? Reason { get; set; }
    }
}
