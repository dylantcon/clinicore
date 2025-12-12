using Core.CliniCore.Api;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.DTOs.Patients;
using Core.CliniCore.Mapping;
using Core.CliniCore.Requests.Patients;

namespace Core.CliniCore.Repositories.Remote
{
    /// <summary>
    /// Remote repository implementation that calls the API for patient operations.
    /// Uses ApiRoutes for all endpoint paths (single source of truth).
    /// </summary>
    public class RemotePatientRepository(HttpClient httpClient) : RemoteRepositoryBase(httpClient), IPatientRepository
    {
        public PatientProfile? GetById(Guid id)
        {
            var dto = RepositoryOperationException.ThrowIfNullOperation(
                Get<PatientDto>(ApiRoutes.Patients.GetById(id)), 
                id, 
                "Get");
            
            return dto?.ToDomain();
        }

        public IEnumerable<PatientProfile> GetAll()
        {
            var dtos = RepositoryOperationException.ThrowIfNullOperation(
                GetList<PatientDto>(ApiRoutes.Patients.GetAll()),
                nameof(PatientDto),
                "GetAll");

            return dtos.Select(dto => dto.ToDomain());
        }

        public void Add(PatientProfile entity)
        {
            var request = new CreatePatientRequest
            {
                Id = entity.Id,
                Username = entity.Username,
                Name = entity.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty,
                BirthDate = entity.GetValue<DateTime>(CommonEntryType.BirthDate.GetKey()),
                Gender = entity.GetValue<Gender>(PatientEntryType.Gender.GetKey()).ToString(),
                Race = entity.GetValue<string>(PatientEntryType.Race.GetKey()) ?? string.Empty,
                Address = entity.GetValue<string>(CommonEntryType.Address.GetKey()) ?? string.Empty,
                PrimaryPhysicianId = entity.PrimaryPhysicianId
            };

            RepositoryOperationException.ThrowIfNullOperation(
                Post<CreatePatientRequest, PatientDto>(ApiRoutes.Patients.GetAll(), request), 
                entity.Id, 
                "Post");
        }

        public void Update(PatientProfile entity)
        {
            var request = new UpdatePatientRequest
            {
                Name = entity.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty,
                BirthDate = entity.GetValue<DateTime>(CommonEntryType.BirthDate.GetKey()),
                Gender = entity.GetValue<Gender>(PatientEntryType.Gender.GetKey()).ToString(),
                Race = entity.GetValue<string>(PatientEntryType.Race.GetKey()) ?? string.Empty,
                Address = entity.GetValue<string>(CommonEntryType.Address.GetKey()) ?? string.Empty,
                PrimaryPhysicianId = entity.PrimaryPhysicianId
            };

            RepositoryOperationException.ThrowIfNullOperation(
                Put<UpdatePatientRequest, PatientDto>(ApiRoutes.Patients.GetById(entity.Id), request), 
                entity.Id, 
                "Put");
        }

        public void Delete(Guid id)
        {
            RepositoryOperationException.ThrowIfNullOperation(
                Delete<PatientDto>(ApiRoutes.Patients.GetById(id)), 
                id, 
                "Delete");
        }

        public IEnumerable<PatientProfile> Search(string query)
        {
            var dtos = RepositoryOperationException.ThrowIfNullOperation(
                GetList<PatientDto>(ApiRoutes.Patients.SearchByQuery(query)),
                nameof(PatientDto),
                "Search");

            return dtos.Select(dto => dto.ToDomain());
        }

        /// <summary>
        /// Inefficient search by username (client-side filtering)
        /// TODO: Add dedicated API endpoint for this
        /// </summary>
        /// <param name="username"></param>
        /// <returns>A nullable patient profile clientside data model.</returns>
        public PatientProfile? GetByUsername(string username)
        {
            // Search and filter client-side (API doesn't have dedicated endpoint)
            IEnumerable<PatientProfile> all = GetAll();
            return all.FirstOrDefault(p =>
                p.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<PatientProfile> GetByPhysician(Guid physicianId)
        {
            var dtos = RepositoryOperationException.ThrowIfNullOperation(
                GetList<PatientDto>(ApiRoutes.Patients.GetByPhysician(physicianId)),
                nameof(PatientDto),
                "GetByPhysician");

            return dtos.Select(dto => dto.ToDomain());
        }

        public IEnumerable<PatientProfile> GetUnassigned()
        {
            var dtos = RepositoryOperationException.ThrowIfNullOperation(
                GetList<PatientDto>(ApiRoutes.Patients.GetUnassigned()),
                nameof(PatientDto),
                "GetUnassigned");

            return dtos.Select(dto => dto.ToDomain());
        }
    }
}
