namespace Backend.Fx.Persistence
{
    /// <summary>
    /// Encapsulates database bootstrapping. This interface hides the implementation details for creating/migrating the database
    /// </summary>
    public interface IDatabaseBootstrapper : IDisposable
    {
        Task EnsureDatabaseExistenceAsync(CancellationToken cancellationToken);
    }
}