using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TripleP.Backend.Database.Context;
using TripleP.Backend.Database.Identity;

namespace TripleP.Backend.Tests.Database;

/// <summary>
/// Drops, recreates and migrates the PostgreSQL test database once per test collection.
/// The connection is read from <c>ConnectionStrings:TestPostgresConnection</c>, falling back to
/// <c>ConnectionStrings:PostgresConnection</c>; only the host, port and credentials are used and
/// the database name is always forced to <c>triplep_test</c> so real data is never touched.
/// </summary>
public sealed class PostgresDatabaseFixture : IAsyncLifetime
{
    public const string DatabaseName = "triplep_test";

    public TriplePDbContext Context { get; private set; } = null!;

    public ServiceProvider Services { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var connectionString = BuildConnectionString();

        var options = new DbContextOptionsBuilder<TriplePDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        Context = new TriplePDbContext(options);

        // A from-scratch database proves that the committed migrations create the schema.
        await Context.Database.EnsureDeletedAsync();
        await Context.Database.MigrateAsync();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<TriplePDbContext>(builder => builder.UseNpgsql(connectionString));
        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<TriplePDbContext>();
        Services = services.BuildServiceProvider();
    }

    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
        await Services.DisposeAsync();
    }

    private static string BuildConnectionString()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(typeof(PostgresDatabaseFixture).Assembly, optional: true)
            .Build();

        var connectionString =
            configuration.GetConnectionString("TestPostgresConnection")
            ?? configuration.GetConnectionString("PostgresConnection")
            ?? throw new InvalidOperationException(
                "PostgreSQL connection string is missing. Set 'ConnectionStrings:PostgresConnection' " +
                "through environment variables or user secrets before running the database tests.");

        return new NpgsqlConnectionStringBuilder(connectionString)
        {
            Database = DatabaseName,
        }.ConnectionString;
    }
}
