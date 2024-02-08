using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Backend.Fx.Persistence.Abstractions;

[PublicAPI]
public interface IDatabaseAvailabilityAwaiter
{
    void WaitForDatabase();
    
    Task WaitForDatabaseAsync(CancellationToken cancellationToken);
}


[PublicAPI]
public class DatabaseAvailabilityAwaiter : IDatabaseAvailabilityAwaiter
{
    private readonly IDbUtil _dbUtil;

    public DatabaseAvailabilityAwaiter(IDbUtil dbUtil)
    {
        _dbUtil = dbUtil;
    }

    public void WaitForDatabase()
    {
        _dbUtil.WaitUntilAvailable();
    }

    public Task WaitForDatabaseAsync(CancellationToken cancellationToken)
    {
        return _dbUtil.WaitUntilAvailableAsync(cancellationToken);
    }
}