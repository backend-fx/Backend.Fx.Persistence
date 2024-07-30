using System.Data;
using Backend.Fx.Execution.DependencyInjection;
using Backend.Fx.Execution.Pipeline;
using Backend.Fx.Persistence.AdoNet;
using Backend.Fx.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Persistence.Feature;

public class PersistenceModule(IDbConnectionFactory dbConnectionFactory, bool enableTransactions) : IModule
{
    public void Register(ICompositionRoot compositionRoot)
    {
        // singleton db connection factory
        compositionRoot.Register(ServiceDescriptor.Singleton(dbConnectionFactory));

        // scoped db connections are provided by the connection factory
        compositionRoot.Register(
            ServiceDescriptor.Scoped<IDbConnection>(sp => sp.GetRequiredService<IDbConnectionFactory>().Create()));

        // decorator: automatic connection opening and closing
        compositionRoot.RegisterDecorator(ServiceDescriptor.Scoped<IOperation, DbConnectionOperationDecorator>());

        if (enableTransactions)
        {
            // decorator: automatic transactions
            compositionRoot.Register(
                ServiceDescriptor.Scoped<ICurrentTHolder<IDbTransaction>, CurrentDbTransactionHolder>());
            compositionRoot.RegisterDecorator(ServiceDescriptor.Scoped<IOperation, DbTransactionOperationDecorator>());
        }
    }
}
