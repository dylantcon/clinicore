namespace Core.CliniCore.Repositories
{
    /// <summary>
    /// Exception thrown when a repository operation (Add, Update, Delete) fails.
    /// Provides context about which operation failed and on what entity.
    /// </summary>
    public class RepositoryOperationException : Exception
    {
        /// <summary>
        /// The operation that failed (e.g., "Add", "Update", "Delete")
        /// </summary>
        public string Operation { get; }

        /// <summary>
        /// The type of entity involved (e.g., "Patient", "ClinicalDocument")
        /// </summary>
        public string EntityType { get; }

        /// <summary>
        /// The ID of the entity, if available
        /// </summary>
        public Guid? EntityId { get; }

        public RepositoryOperationException(string operation, string entityType, Guid? entityId, string message)
            : base(message)
        {
            Operation = operation;
            EntityType = entityType;
            EntityId = entityId;
        }

        public RepositoryOperationException(string operation, string entityType, Guid? entityId, string message, Exception innerException)
            : base(message, innerException)
        {
            Operation = operation;
            EntityType = entityType;
            EntityId = entityId;
        }

        public override string ToString()
        {
            var idPart = EntityId.HasValue ? $" (ID: {EntityId.Value})" : "";
            return $"{Operation} operation failed for {EntityType}{idPart}: {Message}";
        }
    }
}
