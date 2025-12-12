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
    public class RemotePatientRepository : RemoteRepositoryBase, IPatientRepository
    {
        public RemotePatientRepository(HttpClient httpClient) : base(httpClient)
        {
        }

        public PatientProfile? GetById(Guid id)
        {
            var dto = Get<PatientDto>(ApiRoutes.Patients.GetById(id));
            return dto?.ToDomain();
        }

        public IEnumerable<PatientProfile> GetAll()
        {
            var dtos = GetList<PatientDto>(ApiRoutes.Patients.GetAll());
            return dtos.Select(d => d.ToDomain());
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

            var result = Post<CreatePatientRequest, PatientDto>(ApiRoutes.Patients.GetAll(), request);
            if (result == null)
                throw new RepositoryOperationException("Add", "Patient", entity.Id, "Remote server failed to create the patient");
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

            var result = Put(ApiRoutes.Patients.GetById(entity.Id), request);
            if (!result)
                throw new RepositoryOperationException("Update", "Patient", entity.Id, "Remote server failed to update the patient");
        }

        public void Delete(Guid id)
        {
            var result = Delete(ApiRoutes.Patients.GetById(id));
            if (!result)
                throw new RepositoryOperationException("Delete", "Patient", id, "Remote server failed to delete the patient");
        }

        public IEnumerable<PatientProfile> Search(string query)
        {
            var dtos = GetList<PatientDto>(ApiRoutes.Patients.SearchByQuery(query));
            return dtos.Select(d => d.ToDomain());
        }

        public PatientProfile? GetByUsername(string username)
        {
            // Search and filter client-side (API doesn't have dedicated endpoint)
            var all = GetAll();
            return all.FirstOrDefault(p =>
                p.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<PatientProfile> GetByPhysician(Guid physicianId)
        {
            var dtos = GetList<PatientDto>(ApiRoutes.Patients.GetByPhysician(physicianId));
            return dtos.Select(d => d.ToDomain());
        }

        public IEnumerable<PatientProfile> GetUnassigned()
        {
            var dtos = GetList<PatientDto>(ApiRoutes.Patients.GetUnassigned());
            return dtos.Select(d => d.ToDomain());
        }
    }
}
