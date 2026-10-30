using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TripleP.Backend.Database.Context;
using TripleP.Backend.Database.Identity;

namespace TripleP.Backend.Tests.Database;

/// <summary>
/// Test composition root. It loads <c>appsettings.json</c> (environment variables override it) and
/// registers the real services through <c>AddTriplePDatabase</c>, plus the Identity services the
/// web host registers in <c>Program.cs</c>, so tests exercise the same wiring as the application.
/// The database itself is never created or migrated here.
/// </summary>
public sealed class Startup : IDisposable
{
    public Startup()
    {
        Configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTriplePDatabase(Configuration);

        // Identity now lives in the web composition root; register it here as well so the
        // database tests get the same UserManager/store wiring as the application.
        services
            .AddIdentityApiEndpoints<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<TriplePDbContext>();

        Services = services.BuildServiceProvider();
    }

    public IConfiguration Configuration { get; }

    public ServiceProvider Services { get; }

    public void Dispose() => Services.Dispose();
}
