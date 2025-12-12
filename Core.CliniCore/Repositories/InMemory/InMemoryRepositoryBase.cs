using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Users;

namespace Core.CliniCore.Repositories.InMemory
{
    /// <summary>
    /// Abstract base class for in-memory repository implementations.
    /// Provides thread-safe CRUD operations using a dictionary backing store.
    ///
    /// Design Notes:
    /// - Uses lock-based synchronization for thread safety
    /// - Dictionary keyed by entity Id for O(1) lookups
    /// - Derived classes implement Search() method for entity-specific search logic
    /// </summary>
    /// <typeparam name="T">Entity type implementing IIdentifiable</typeparam>
    public abstract class InMemoryRepositoryBase<T> : IRepository<T> where T : class, IIdentifiable
    {
        protected readonly Dictionary<Guid, T> _entities = new();
        protected readonly object _lock = new();

        /// <summary>
        /// Gets an entity by its unique identifier
        /// </summary>
        /// <param name="id">The entity's GUID</param>
        /// <returns>The entity if found, null otherwise</returns>
        public virtual T? GetById(Guid id)
        {
            lock (_lock)
            {
                return _entities.TryGetValue(id, out var entity) ? entity : null;
            }
        }

        /// <summary>
        /// Gets all entities in the repository
        /// </summary>
        /// <returns>An enumerable of all entities (snapshot at time of call)</returns>
        public virtual IEnumerable<T> GetAll()
        {
            lock (_lock)
            {
                // Return a copy to prevent modification during iteration
                return _entities.Values.ToList();
            }
        }

        /// <summary>
        /// Adds a new entity to the repository
        /// </summary>
        /// <param name="entity">The entity to add</param>
        /// <exception cref="ArgumentNullException">If entity is null</exception>
        /// <exception cref="InvalidOperationException">If entity with same Id already exists</exception>
        public virtual void Add(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            lock (_lock)
            {
                if (_entities.ContainsKey(entity.Id))
                    throw new InvalidOperationException($"Entity with Id {entity.Id} already exists");

                _entities[entity.Id] = entity;
            }
        }

        /// <summary>
        /// Updates an existing entity in the repository
        /// </summary>
        /// <param name="entity">The entity with updated values</param>
        /// <exception cref="ArgumentNullException">If entity is null</exception>
        /// <exception cref="KeyNotFoundException">If entity doesn't exist</exception>
        public virtual void Update(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            lock (_lock)
            {
                if (!_entities.ContainsKey(entity.Id))
                    throw new KeyNotFoundException($"Entity with Id {entity.Id} not found");

                _entities[entity.Id] = entity;
            }
        }

        /// <summary>
        /// Deletes an entity from the repository by Id
        /// </summary>
        /// <param name="id">The Id of the entity to delete</param>
        /// <exception cref="KeyNotFoundException">If entity doesn't exist</exception>
        public virtual void Delete(Guid id)
        {
            lock (_lock)
            {
                if (!_entities.Remove(id))
                    throw new KeyNotFoundException($"Entity with Id {id} not found");
            }
        }

        /// <summary>
        /// Searches for entities matching the query string.
        /// Must be implemented by derived classes for entity-specific search logic.
        /// </summary>
        /// <param name="query">The search query string</param>
        /// <returns>Entities matching the query</returns>
        public abstract IEnumerable<T> Search(string query);

        /// <summary>
        /// Gets the count of entities in the repository
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _entities.Count;
                }
            }
        }

        /// <summary>
        /// Checks if an entity with the given Id exists
        /// </summary>
        public bool Exists(Guid id)
        {
            lock (_lock)
            {
                return _entities.ContainsKey(id);
            }
        }
    }
}
