using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Persistence.Abstractions;
using JetBrains.Annotations;

namespace Backend.Fx.Persistence.AdoNet;

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