using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Backend.Fx.Persistence.Abstractions;

[PublicAPI]
public interface IDatabaseAvailabilityAwaiter
{
    Task WaitForDatabase(CancellationToken cancellationToken);
}