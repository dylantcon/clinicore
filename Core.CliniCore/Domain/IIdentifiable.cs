namespace Core.CliniCore.Domain
{
    /// <summary>
    /// Marker interface for entities that have a unique identifier
    /// </summary>
    public interface IIdentifiable
    {
        Guid Id { get; }
    }
}
