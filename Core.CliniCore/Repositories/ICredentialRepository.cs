using Core.CliniCore.Domain.Authentication.Representation;

namespace Core.CliniCore.Repositories
{
    /// <summary>
    /// Repository interface for user credential operations.
    /// Extends generic repository with credential-specific queries.
    /// </summary>
    public interface ICredentialRepository : IRepository<UserCredential>
    {
        /// <summary>
        /// Gets a credential by username (the natural key for authentication).
        /// </summary>
        UserCredential? GetByUsername(string username);

        /// <summary>
        /// Checks if a username already exists.
        /// </summary>
        bool Exists(string username);

        /// <summary>
        /// Registers a new user credential with password hashing.
        /// Remote repositories call the API; local repositories hash and store.
        /// </summary>
        /// <returns>The created credential, or null if registration failed.</returns>
        UserCredential? Register(Guid id, string username, string password, string role);
    }
}
