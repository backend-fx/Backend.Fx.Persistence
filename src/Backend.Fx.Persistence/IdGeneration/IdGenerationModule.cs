using Backend.Fx.Execution.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Persistence.IdGeneration;

public class IdGenerationModule<TId, TIdGenerator> : IModule 
    where TIdGenerator : class, IEntityIdGenerator<TId> 
{
    private readonly TIdGenerator _entityIdGenerator;

    public IdGenerationModule(TIdGenerator entityIdGenerator)
    {
        _entityIdGenerator = entityIdGenerator;
    }

    public void Register(ICompositionRoot compositionRoot)
    {
        compositionRoot.Register(ServiceDescriptor.Singleton(_entityIdGenerator));
    }
}