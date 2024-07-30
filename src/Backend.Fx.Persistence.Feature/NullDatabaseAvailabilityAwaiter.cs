using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Persistence.Abstractions;
using NodaTime;

namespace Backend.Fx.Persistence.Feature;

public class NullDatabaseAvailabilityAwaiter : IDatabaseAvailabilityAwaiter
{
    public void WaitForDatabase(Duration? timeout = null)
    {
    }

    public Task WaitForDatabaseAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
