using Core.CliniCore.Api;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.DTOs.Appointments;
using Core.CliniCore.Mapping;
using Core.CliniCore.Requests.Appointments;
using Core.CliniCore.Scheduling;

namespace Core.CliniCore.Repositories.Remote
{
    /// <summary>
    /// Remote repository implementation that calls the API for appointment operations.
    /// Uses ApiRoutes for all endpoint paths (single source of truth).
    /// </summary>
    public class RemoteAppointmentRepository(HttpClient httpClient) : RemoteRepositoryBase(httpClient), IAppointmentRepository
    {
        public AppointmentTimeInterval? GetById(Guid id)
        {
            var dto = Get<AppointmentDto>(ApiRoutes.Appointments.GetById(id));
            return dto?.ToDomain();
        }

        public IEnumerable<AppointmentTimeInterval> GetAll()
        {
            var dtos = GetList<AppointmentDto>(ApiRoutes.Appointments.GetAll());
            return dtos.Select(d => d.ToDomain());
        }

        public void Add(AppointmentTimeInterval entity)
        {
            var request = new CreateAppointmentRequest
            {
                PatientId = entity.PatientId,
                PhysicianId = entity.PhysicianId,
                Start = entity.Start,
                End = entity.End,
                ReasonForVisit = entity.ReasonForVisit,
                Notes = entity.Notes,
                RoomNumber = entity.RoomNumber
            };

            var result = Post<CreateAppointmentRequest, AppointmentDto>(ApiRoutes.Appointments.GetAll(), request);
            if (result == null)
            {
                throw new RepositoryOperationException(
                    "Add",
                    "Appointment",
                    null,
                    "Failed to create appointment. The API request did not return a valid response.");
            }
        }

        public void Update(AppointmentTimeInterval entity)
        {
            var request = new UpdateAppointmentRequest
            {
                Start = entity.Start,
                End = entity.End,
                Status = entity.Status.ToString(),
                ReasonForVisit = entity.ReasonForVisit,
                Notes = entity.Notes,
                CancellationReason = entity.CancellationReason,
                RoomNumber = entity.RoomNumber,
                ModifiedAt = entity.ModifiedAt
            };

            var success = Put(ApiRoutes.Appointments.GetById(entity.Id), request);
            if (!success)
            {
                throw new RepositoryOperationException(
                    "Update",
                    "Appointment",
                    entity.Id,
                    $"Failed to update appointment with ID {entity.Id}. The API request was not successful.");
            }
        }

        public void Delete(Guid id)
        {
            var success = Delete(ApiRoutes.Appointments.GetById(id));
            if (!success)
            {
                throw new RepositoryOperationException(
                    "Delete",
                    "Appointment",
                    id,
                    $"Failed to delete appointment with ID {id}. The API request was not successful.");
            }
        }

        public IEnumerable<AppointmentTimeInterval> Search(string query)
        {
            var dtos = GetList<AppointmentDto>($"{ApiRoutes.Appointments.BasePath}/search?q={Uri.EscapeDataString(query)}");
            return dtos.Select(d => d.ToDomain());
        }

        public IEnumerable<AppointmentTimeInterval> GetByDate(DateTime date)
        {
            var dtos = GetList<AppointmentDto>(ApiRoutes.Appointments.GetByDate(date));
            return dtos.Select(d => d.ToDomain());
        }

        public IEnumerable<AppointmentTimeInterval> GetByPhysician(Guid physicianId)
        {
            var dtos = GetList<AppointmentDto>(ApiRoutes.Appointments.GetByPhysician(physicianId));
            return dtos.Select(d => d.ToDomain());
        }

        public IEnumerable<AppointmentTimeInterval> GetByPatient(Guid patientId)
        {
            var dtos = GetList<AppointmentDto>(ApiRoutes.Appointments.GetByPatient(patientId));
            return dtos.Select(d => d.ToDomain());
        }

        public IEnumerable<AppointmentTimeInterval> GetByStatus(AppointmentStatus status)
        {
            var dtos = GetList<AppointmentDto>(ApiRoutes.Appointments.GetByStatus(status.ToString()));
            return dtos.Select(d => d.ToDomain());
        }

        public bool HasConflict(Guid physicianId, DateTime start, TimeSpan duration, Guid? excludeAppointmentId = null)
        {
            var url = ApiRoutes.Appointments.CheckConflict(physicianId, start, (int)duration.TotalMinutes, excludeAppointmentId);
            var result = Get<ConflictCheckResponse>(url);
            return result?.HasConflict ?? false;
        }

        public IEnumerable<(DateTime Start, DateTime End)> GetAvailableSlots(Guid physicianId, DateTime date, TimeSpan duration)
        {
            var dtos = GetList<AvailableSlotDto>(
                ApiRoutes.Appointments.GetAvailableSlots(physicianId, date, (int)duration.TotalMinutes));
            return dtos.Select(d => (d.Start, d.End));
        }

        // Response models for specific endpoints
        private record ConflictCheckResponse(bool HasConflict, string? Message);
        private record AvailableSlotDto(DateTime Start, DateTime End);
    }
}
