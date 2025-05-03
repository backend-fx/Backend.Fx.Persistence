using System;
using System.Data.Common;
using Backend.Fx.Persistence.Postgres.Sequences;
using JetBrains.Annotations;
using NodaTime;
using Npgsql;

namespace Backend.Fx.Persistence.Postgres;

[PublicAPI]
public abstract class PostgresTestDatabaseBuilder
{
    public virtual string DatabaseNamePrefix => "test";

    public virtual string HostName => "localhost";

    public virtual int Port => 5432;

    public abstract string Username { get; }

    public abstract string Password { get; }

    public abstract string PostgresPassword { get; }

    public PostgresTestDatabase Build()
    {
        return new PostgresTestDatabase(
            Username,
            Password,
            DatabaseNamePrefix,
            new NpgsqlConnectionStringBuilder
            {
                Host = HostName,
                Port = Port,
                Username = "postgres",
                Password = PostgresPassword,
                PersistSecurityInfo = true,
                IncludeErrorDetail = true
            });
    }
}


/// <summary>
/// Waits max 10 secs for the postgres database, creates a new database with a unique name. A maximum of 100 databases
/// is kept, until databases are overwritten.
/// </summary>
[PublicAPI]
public class PostgresTestDatabase
{
    public string TestDbName { get; }

    public NpgsqlDataSource DataSource { get; }

    public PostgresTestDatabase(
        string username,
        string password,
        string databaseNamePrefix,
        NpgsqlConnectionStringBuilder connectionStringBuilder)
    {
        string connectionString = connectionStringBuilder.ConnectionString;

        new PostgresTcpSocketAvailabilityAwaiter(connectionString)
            .WaitForDatabase(Duration.FromSeconds(10));

        // use the system connection string to drop and create the database
        {
            var adminDataSource = NpgsqlDataSource.Create(connectionStringBuilder);
            TestDbName = TestDbNameGenerator.GetNextDbName(adminDataSource, databaseNamePrefix);
            var postgresUtil = new PostgresDbUtil(adminDataSource);
            postgresUtil.EnsureDroppedDatabase(TestDbName);
            postgresUtil.CreateDatabase(TestDbName);

            using var dbConnection = adminDataSource.OpenConnection();
            using var cmd = dbConnection.CreateCommand();
            cmd.CommandText = $"GRANT ALL PRIVILEGES ON DATABASE {TestDbName} TO {username};";
            cmd.ExecuteNonQuery();
        }

        connectionStringBuilder.Username = username;
        connectionStringBuilder.Password = password;
        connectionStringBuilder.Database = TestDbName;
        ConnectionStringBuilder = connectionStringBuilder;

        var builder = new NpgsqlDataSourceBuilder(ConnectionStringBuilder.ConnectionString);
        builder.UseNodaTime();
        DataSource = builder.Build();

        // check connection as app user (not "postgres")
        new PostgresTcpSocketAvailabilityAwaiter(connectionStringBuilder.ConnectionString)
            .WaitForDatabase(Duration.FromSeconds(10));
    }

    public NpgsqlConnectionStringBuilder ConnectionStringBuilder { get; }


    private static class TestDbNameGenerator
    {
        private static readonly object SyncLock = new();

        private static NextTestDbNumSequence? _sequence;

        /// <summary>
        /// This method uses a sequence to create a <i>somewhat</i> unique test databases name. What means "somewhat unique"?
        /// Well, we do not want to increase the number endlessly, but we must also ensure that tests running in parallel do
        /// not interfere with each other. So we apply a modulo 100 to the sequence value. This results in up to 100
        /// databases named <c>mep_test_{00..99}</c>, providing enough spread to prevent interference (there is still some
        /// time to pass until we have >100 core CPUs) and also we do not overload the postgres pod with endless databases.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        ///     <item>
        ///         <description>The caller is responsible for deleting a possible existing database having the same name.</description>
        ///     </item>
        ///     <item>
        ///         <description>The method is thread-safe.</description>
        ///     </item>
        /// </list>
        /// </remarks>
        public static string GetNextDbName(DbDataSource dbDataSource, string databaseNamePrefix)
        {
            lock (SyncLock)
            {
                if (_sequence == null)
                {
                    _sequence = new NextTestDbNumSequence(dbDataSource);
                    _sequence.EnsureExistence();
                }

                return $"{databaseNamePrefix}_{_sequence.GetNextValue() % 100:00}";
            }
        }


        private class NextTestDbNumSequence : PostgresSequence<int>
        {
            private readonly DbDataSource _dbDataSource;

            public NextTestDbNumSequence(DbDataSource dbDataSource, int startWith = 1)
                : base(dbDataSource, startWith)
            {
                _dbDataSource = dbDataSource;
            }

            protected override int ConvertNextValueFromSequence(object valueFromSequence)
            {
                return Convert.ToInt32(valueFromSequence);
            }

            protected override string SequenceName => "next_test_db_num";

            protected override string SchemaName => "public";

            public override int Increment => 1;

            public void EnsureExistence()
            {
                using var dbConnection = _dbDataSource.CreateConnection();
                dbConnection.Open();

                using var cmd = dbConnection.CreateCommand();
                cmd.CommandText =
                    $"""
                     DO $$
                     BEGIN
                        IF NOT EXISTS (
                            SELECT 1
                            FROM information_schema.sequences
                            WHERE sequence_schema = '{SchemaName}' AND sequence_name = '{SequenceName}'
                        ) THEN
                           EXECUTE 'CREATE SEQUENCE {SchemaName}.{SequenceName}';
                        END IF;
                     END;
                     $$;
                     """;

                cmd.ExecuteNonQuery();
            }
        }
    }
}
