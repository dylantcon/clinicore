using Core.CliniCore.Api;
using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.DTOs.Auth;
using Core.CliniCore.Mapping;
using Core.CliniCore.Requests.Auth;

namespace Core.CliniCore.Repositories.Remote
{
    /// <summary>
    /// Remote repository implementation that calls the API for credential operations.
    /// Unlike other remote repos, credentials require special auth endpoints.
    /// Uses ApiRoutes for all endpoint paths (single source of truth).
    /// </summary>
    public class RemoteCredentialRepository : RemoteRepositoryBase, ICredentialRepository
    {
        public RemoteCredentialRepository(HttpClient httpClient) : base(httpClient)
        {
        }

        public UserCredential? GetById(Guid id)
        {
            var dto = Get<UserCredentialDto>(ApiRoutes.Auth.GetById(id));
            return dto?.ToDomain();
        }

        public UserCredential? GetByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            var dto = Get<UserCredentialDto>(ApiRoutes.Auth.GetByUsername(username));
            return dto?.ToDomain();
        }

        public bool Exists(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            // Use the by-username endpoint and check if result exists
            var credential = GetByUsername(username);
            return credential != null;
        }

        public IEnumerable<UserCredential> GetAll()
        {
            var dtos = GetList<UserCredentialDto>(ApiRoutes.Auth.GetAll());
            return dtos.Select(d => d.ToDomain());
        }

        public void Add(UserCredential entity)
        {
            // Registration requires password, which we don't have from the domain model
            // This method is used internally when seeding; for API we'd use RegisterCredentialRequest
            throw new NotSupportedException(
                "Use RegisterCredentialRequest via POST /api/auth/register for new credentials. " +
                "Direct Add is not supported for remote repositories.");
        }

        public void Update(UserCredential entity)
        {
            var dto = entity.ToDto();
            if (!Put(ApiRoutes.Auth.GetById(entity.Id), dto))
            {
                throw new RepositoryOperationException("Update", "Credential", entity.Id, $"Failed to update credential for user {entity.Username} on remote server");
            }
        }

        public void Delete(Guid id)
        {
            if (!Delete(ApiRoutes.Auth.GetById(id)))
            {
                throw new RepositoryOperationException("Delete", "Credential", id, $"Failed to delete credential {id} on remote server");
            }
        }

        public IEnumerable<UserCredential> Search(string query)
        {
            var dtos = GetList<UserCredentialDto>(ApiRoutes.Auth.SearchByQuery(query));
            return dtos.Select(d => d.ToDomain());
        }

        /// <summary>
        /// Validates credentials against the API.
        /// Returns the credential if valid, null if invalid.
        /// </summary>
        public UserCredential? ValidateCredentials(string username, string password)
        {
            var request = new ValidateCredentialRequest
            {
                Username = username,
                Password = password
            };

            var dto = Post<ValidateCredentialRequest, UserCredentialDto>(ApiRoutes.Auth.ValidateCredentials(), request);
            return dto?.ToDomain();
        }

        /// <summary>
        /// Registers new credentials via the API.
        /// </summary>
        public UserCredential? Register(Guid profileId, string username, string password, string role)
        {
            var request = new RegisterCredentialRequest
            {
                Id = profileId,
                Username = username,
                Password = password,
                Role = role
            };

            var dto = Post<RegisterCredentialRequest, UserCredentialDto>(ApiRoutes.Auth.RegisterCredentials(), request);
            return dto?.ToDomain();
        }

        /// <summary>
        /// Changes a user's password via the API.
        /// </summary>
        public bool ChangePassword(string username, string currentPassword, string newPassword)
        {
            var request = new ChangePasswordRequest
            {
                Username = username,
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            };

            try
            {
                Put(ApiRoutes.Auth.ChangeUserPassword(), request);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Locks a user account.
        /// </summary>
        public bool LockAccount(string username)
        {
            try
            {
                Put(ApiRoutes.Auth.LockAccount(username), new { });
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Unlocks a user account.
        /// </summary>
        public bool UnlockAccount(string username)
        {
            try
            {
                Put(ApiRoutes.Auth.UnlockAccount(username), new { });
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
