using Core.CliniCore.Api;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.DTOs.Administrators;
using Core.CliniCore.Mapping;
using Core.CliniCore.Requests.Administrators;

namespace Core.CliniCore.Repositories.Remote
{
    /// <summary>
    /// Remote repository implementation that calls the API for administrator operations.
    /// Uses ApiRoutes for all endpoint paths (single source of truth).
    /// </summary>
    public class RemoteAdministratorRepository : RemoteRepositoryBase, IAdministratorRepository
    {
        public RemoteAdministratorRepository(HttpClient httpClient) : base(httpClient)
        {
        }

        public AdministratorProfile? GetById(Guid id)
        {
            var dto = RepositoryOperationException.ThrowIfNullOperation(
                Get<AdministratorDto>(ApiRoutes.Administrators.GetById(id)),
                id,
                "Get");

            return dto?.ToDomain();
        }

        public IEnumerable<AdministratorProfile> GetAll()
        {
            var dtos = RepositoryOperationException.ThrowIfNullOperation(
                GetList<AdministratorDto>(ApiRoutes.Administrators.GetAll()),
                nameof(AdministratorDto),
                "GetAll");

            return dtos.Select(dto => dto.ToDomain());
        }

        public void Add(AdministratorProfile entity)
        {
            var request = new CreateAdministratorRequest
            {
                Id = entity.Id,
                Username = entity.Username,
                Name = entity.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty,
                Address = entity.GetValue<string>(CommonEntryType.Address.GetKey()) ?? string.Empty,
                BirthDate = entity.GetValue<DateTime>(CommonEntryType.BirthDate.GetKey()),
                Email = entity.GetValue<string>(AdministratorEntryType.Email.GetKey()),
                Department = entity.Department,
                Permissions = entity.GrantedPermissions.Select(p => p.ToString()).ToList()
            };

            RepositoryOperationException.ThrowIfNullOperation(
                Post<CreateAdministratorRequest, AdministratorDto>(ApiRoutes.Administrators.GetAll(), request),
                entity.Id,
                "Post");
        }

        public void Update(AdministratorProfile entity)
        {
            var birthDate = entity.GetValue<DateTime>(CommonEntryType.BirthDate.GetKey());
            var request = new UpdateAdministratorRequest
            {
                Name = entity.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty,
                Address = entity.GetValue<string>(CommonEntryType.Address.GetKey()) ?? string.Empty,
                BirthDate = birthDate != default ? birthDate : null,
                Email = entity.GetValue<string>(AdministratorEntryType.Email.GetKey()),
                Department = entity.Department,
                Permissions = entity.GrantedPermissions.Select(p => p.ToString()).ToList()
            };

            var success = Put(ApiRoutes.Administrators.GetById(entity.Id), request);
            if (!success)
            {
                throw new RepositoryOperationException("Update", "Administrator", entity.Id, $"Failed to update administrator with ID '{entity.Id}'.");
            }
        }

        public void Delete(Guid id)
        {
            var success = Delete(ApiRoutes.Administrators.GetById(id));
            if (!success)
            {
                throw new RepositoryOperationException("Delete", "Administrator", id, $"Failed to delete administrator with ID '{id}'.");
            }
        }

        public IEnumerable<AdministratorProfile> Search(string query)
        {
            // Search client-side since API doesn't have dedicated search endpoint
            var all = GetAll();
            return all.Where(a =>
                (a.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty).Contains(query, StringComparison.OrdinalIgnoreCase) ||
                a.Username.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                a.Department.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        public AdministratorProfile? GetByUsername(string username)
        {
            // Search and filter client-side (API doesn't have dedicated endpoint)
            var all = GetAll();
            return all.FirstOrDefault(a =>
                a.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<AdministratorProfile> GetByDepartment(string department)
        {
            var dtos = RepositoryOperationException.ThrowIfNullOperation(
                GetList<AdministratorDto>(ApiRoutes.Administrators.GetByDepartment(department)),
                nameof(AdministratorDto),
                "GetByDepartment");

            return dtos.Select(dto => dto.ToDomain());
        }

        public IEnumerable<AdministratorProfile> GetByPermission(Permission permission)
        {
            var dtos = RepositoryOperationException.ThrowIfNullOperation(
                GetList<AdministratorDto>(ApiRoutes.Administrators.GetByPermission(permission.ToString())),
                nameof(AdministratorDto),
                "GetByPermission");

            return dtos.Select(dto => dto.ToDomain());
        }
    }
}
