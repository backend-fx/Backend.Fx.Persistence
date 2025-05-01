using System.Data;
using System.Data.Common;
using Backend.Fx.Execution.DependencyInjection;
using Backend.Fx.Execution.Pipeline;
using Backend.Fx.Persistence.AdoNet;
using Backend.Fx.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Persistence.Feature;

public class PersistenceModule(DbDataSource dbDataSource, bool enableTransactions) : IModule
{
    public void Register(ICompositionRoot compositionRoot)
    {
        // singleton db connection factory
        compositionRoot.Register(ServiceDescriptor.Singleton(dbDataSource));

        // scoped db connections are provided by the connection factory
        compositionRoot.Register(
            ServiceDescriptor.Scoped<IDbConnection>(sp => sp.GetRequiredService<DbDataSource>().CreateConnection()));

        if (enableTransactions)
        {
            // decorator: automatic transactions
            compositionRoot.Register(
                ServiceDescriptor.Scoped<ICurrentTHolder<IDbTransaction>, CurrentDbTransactionHolder>());
            compositionRoot.RegisterDecorator(ServiceDescriptor.Scoped<IOperation, DbTransactionOperationDecorator>());
        }

        // decorator: automatic connection opening and closing
        compositionRoot.RegisterDecorator(ServiceDescriptor.Scoped<IOperation, DbConnectionOperationDecorator>());
    }
}
