using Core.CliniCore.Api;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.DTOs.Physicians;
using Core.CliniCore.Mapping;
using Core.CliniCore.Repositories;
using Core.CliniCore.Requests.Physicians;

namespace Core.CliniCore.Repositories.Remote
{
    /// <summary>
    /// Remote repository implementation that calls the API for physician operations.
    /// Uses ApiRoutes for all endpoint paths (single source of truth).
    /// </summary>
    public class RemotePhysicianRepository : RemoteRepositoryBase, IPhysicianRepository
    {
        public RemotePhysicianRepository(HttpClient httpClient) : base(httpClient)
        {
        }

        public PhysicianProfile? GetById(Guid id)
        {
            var dto = RepositoryOperationException.ThrowIfNullOperation(
                Get<PhysicianDto>(ApiRoutes.Physicians.GetById(id)),
                id,
                "Get");

            return dto?.ToDomain();
        }

        public IEnumerable<PhysicianProfile> GetAll()
        {
            var dtos = RepositoryOperationException.ThrowIfNullOperation(
                GetList<PhysicianDto>(ApiRoutes.Physicians.GetAll()),
                nameof(PhysicianDto),
                "GetAll");

            return dtos.Select(dto => dto.ToDomain());
        }

        public void Add(PhysicianProfile entity)
        {
            var request = new CreatePhysicianRequest
            {
                Id = entity.Id,
                Username = entity.Username,
                Name = entity.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty,
                BirthDate = entity.GetValue<DateTime>(CommonEntryType.BirthDate.GetKey()),
                Address = entity.GetValue<string>(CommonEntryType.Address.GetKey()),
                LicenseNumber = entity.GetValue<string>(PhysicianEntryType.LicenseNumber.GetKey()) ?? string.Empty,
                GraduationDate = entity.GetValue<DateTime>(PhysicianEntryType.GraduationDate.GetKey()),
                Specializations = (entity.GetValue<List<MedicalSpecialization>>(PhysicianEntryType.Specializations.GetKey()) ?? new())
                    .Select(s => s.ToString()).ToList()
            };

            RepositoryOperationException.ThrowIfNullOperation(
                Post<CreatePhysicianRequest, PhysicianDto>(ApiRoutes.Physicians.GetAll(), request),
                entity.Id,
                "Post");
        }

        public void Update(PhysicianProfile entity)
        {
            var request = new UpdatePhysicianRequest
            {
                Name = entity.GetValue<string>(CommonEntryType.Name.GetKey()),
                Address = entity.GetValue<string>(CommonEntryType.Address.GetKey()),
                BirthDate = entity.GetValue<DateTime>(CommonEntryType.BirthDate.GetKey()),
                LicenseNumber = entity.GetValue<string>(PhysicianEntryType.LicenseNumber.GetKey()),
                GraduationDate = entity.GetValue<DateTime>(PhysicianEntryType.GraduationDate.GetKey()),
                Specializations = (entity.GetValue<List<MedicalSpecialization>>(PhysicianEntryType.Specializations.GetKey()) ?? new())
                    .Select(s => s.ToString()).ToList(),
                PatientIds = entity.PatientIds.ToList(),
                AppointmentIds = entity.AppointmentIds.ToList()
            };

            var success = Put(ApiRoutes.Physicians.GetById(entity.Id), request);
            if (!success)
            {
                throw new RepositoryOperationException("Update", "Physician", entity.Id, "Failed to update physician.");
            }
        }

        public void Delete(Guid id)
        {
            var success = Delete(ApiRoutes.Physicians.GetById(id));
            if (!success)
            {
                throw new RepositoryOperationException("Delete", "Physician", id, "Failed to delete physician.");
            }
        }

        public IEnumerable<PhysicianProfile> Search(string query)
        {
            var dtos = RepositoryOperationException.ThrowIfNullOperation(
                GetList<PhysicianDto>(ApiRoutes.Physicians.SearchByQuery(query)),
                nameof(PhysicianDto),
                "Search");

            return dtos.Select(dto => dto.ToDomain());
        }

        public PhysicianProfile? GetByUsername(string username)
        {
            // Search and filter client-side
            var all = GetAll();
            return all.FirstOrDefault(p =>
                p.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<PhysicianProfile> FindBySpecialization(MedicalSpecialization spec)
        {
            var dtos = RepositoryOperationException.ThrowIfNullOperation(
                GetList<PhysicianDto>(ApiRoutes.Physicians.GetBySpecialization(spec.ToString())),
                nameof(PhysicianDto),
                "FindBySpecialization");

            return dtos.Select(dto => dto.ToDomain());
        }

        public IEnumerable<PhysicianProfile> GetAvailableOn(DateTime date)
        {
            var dtos = RepositoryOperationException.ThrowIfNullOperation(
                GetList<PhysicianDto>(ApiRoutes.Physicians.GetAvailableOn(date)),
                nameof(PhysicianDto),
                "GetAvailableOn");

            return dtos.Select(dto => dto.ToDomain());
        }
    }
}
