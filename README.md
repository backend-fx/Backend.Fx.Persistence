# Backend.Fx.Persistence

This library addresses recurring challenges when dealing with any sort of persistence in an application.

### Waiting for database availability

By implementing an `IDatabaseAvailabilityAwaiter` the application boot is delayed until the database is responding properly. Some database specific implementations are already provided.

### Creating annd migration database

The interface `IDatabaseBootstrapper` is the foundation for a controlled database creation and migration step during application boot. Implement it with your preferred technique, no matter if it's Entity Framework Migrations, FluentMigrator or any other tooling.

The application boot is delayed until the database is bootstrapped.

Check out the `AdoNetDbUtil` and their concrete implementations for specific database system to do the heavy lifting of creating and dropping databases.

### Id Generation

The [Hi/Lo algorithm](https://en.wikipedia.org/wiki/Hi/Lo_algorithm) using a database sequence as underlying structure is provided for 32 bit and 64 bit integers.

That way you can follow the DDD approach of giving identities to each entity during creation without the cost of maintaining many short lived database connections asking a sequence for each and every new id.

Classes to abstract away sequence generation and access are available for different database management systems.

## Getting started

1. Add a reference to the `Backend.Fx.Persistence` package in your persistence adapter assembly (that's where you would implement database specific repositories)
1. Add a reference to the `Backend.Fx.Persistence.Feature` package in your composition assembly (that's where your `BackendFxApplication` class lives)
1. Enable the feature in the `BackendFxApplication` class

```csharp
public class MyCoolApplication : BackendFxApplication
{
    public MyCoolApplication() : base(new SimpleInjectorCompositionRoot(), new ExceptionLogger(), GetAssemblies())
    {
        CompositionRoot.RegisterModules( ... );

        var persistenceFeature = new PersistenceFeature(
            dbConnectionFactory, 
            /* optional */ databaseAvailabilityAwaiter,
            /* optional */ databaseBootstrapper,
            /* optional, default: true */ enableTransactions: false);

        // you can add Id generator types using injection (of singletons)...
        persistenceFeature.AddIdGenerator<ThisIdGenerator, ThisId>();

        // ...or register a ready built instance
        persistenceFeature.AddIdGenerator(new ThatIdGenerator());

        EnableFeature(persistenceFeature);
    }
}
```



