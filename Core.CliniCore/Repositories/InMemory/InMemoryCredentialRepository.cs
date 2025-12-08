using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Service;

namespace Core.CliniCore.Repositories.InMemory
{
    /// <summary>
    /// In-memory implementation of ICredentialRepository.
    /// Maintains a secondary index by username for O(1) authentication lookups.
    /// </summary>
    public class InMemoryCredentialRepository : InMemoryRepositoryBase<UserCredential>, ICredentialRepository
    {
        // Secondary index for O(1) username lookups
        private readonly Dictionary<string, UserCredential> _byUsername = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets a credential by username (the natural key for authentication).
        /// </summary>
        public UserCredential? GetByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            lock (_lock)
            {
                return _byUsername.TryGetValue(username, out var credential) ? credential : null;
            }
        }

        /// <summary>
        /// Checks if a username already exists.
        /// </summary>
        public bool Exists(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            lock (_lock)
            {
                return _byUsername.ContainsKey(username);
            }
        }

        /// <summary>
        /// Adds a new credential and updates the username index.
        /// </summary>
        public override void Add(UserCredential entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            lock (_lock)
            {
                if (_entities.ContainsKey(entity.Id))
                    throw new InvalidOperationException($"Credential with Id {entity.Id} already exists");

                if (_byUsername.ContainsKey(entity.Username))
                    throw new InvalidOperationException($"Credential with Username '{entity.Username}' already exists");

                _entities[entity.Id] = entity;
                _byUsername[entity.Username] = entity;
            }
        }

        /// <summary>
        /// Updates an existing credential. Username changes are not allowed.
        /// </summary>
        public override void Update(UserCredential entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            lock (_lock)
            {
                if (!_entities.TryGetValue(entity.Id, out var existing))
                    throw new KeyNotFoundException($"Credential with Id {entity.Id} not found");

                // Username should not change (init-only property)
                if (!existing.Username.Equals(entity.Username, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Username cannot be changed");

                _entities[entity.Id] = entity;
                _byUsername[entity.Username] = entity;
            }
        }

        /// <summary>
        /// Deletes a credential and removes from the username index.
        /// </summary>
        public override void Delete(Guid id)
        {
            lock (_lock)
            {
                if (!_entities.TryGetValue(id, out var existing))
                    throw new KeyNotFoundException($"Credential with Id {id} not found");

                _entities.Remove(id);
                _byUsername.Remove(existing.Username);
            }
        }

        /// <summary>
        /// Searches credentials by username (partial match).
        /// </summary>
        public override IEnumerable<UserCredential> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return GetAll();

            var lowerQuery = query.ToLowerInvariant();

            lock (_lock)
            {
                return _entities.Values
                    .Where(c => c.Username.ToLowerInvariant().Contains(lowerQuery))
                    .ToList();
            }
        }

        /// <summary>
        /// Registers a new credential with password hashing.
        /// </summary>
        public UserCredential? Register(Guid id, string username, string password, string role)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            if (Exists(username))
                return null;

            var credential = new UserCredential
            {
                Id = id,
                Username = username,
                PasswordHash = BasicAuthenticationService.HashPassword(password),
                Role = role,
                CreatedAt = DateTime.UtcNow
            };

            Add(credential);
            return credential;
        }
    }
}
