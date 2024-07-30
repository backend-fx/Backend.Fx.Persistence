using Backend.Fx.Execution.DependencyInjection;
using Backend.Fx.Persistence.IdGeneration;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Persistence.Feature;

public class IdGenerationModule<TId> : IModule
{
    private readonly IIdGenerator<TId> _idGenerator;

    public IdGenerationModule(IIdGenerator<TId> idGenerator)
    {
        _idGenerator = idGenerator;
    }

    public void Register(ICompositionRoot compositionRoot)
    {
        compositionRoot.Register(ServiceDescriptor.Singleton(_idGenerator));
    }
}

public class IdGenerationModule<TIdGenerator, TId> : IModule where TIdGenerator : class, IIdGenerator<TId>
{
    public void Register(ICompositionRoot compositionRoot)
    {
        compositionRoot.Register(ServiceDescriptor.Singleton<IIdGenerator<TId>, TIdGenerator>());
    }
}
