using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TripleP.Backend.Tests.Database;

/// <summary>
/// Test composition root. It loads <c>appsettings.json</c> (environment variables override it) and
/// registers the real services through <c>AddTriplePDatabase</c>, so tests exercise the same wiring
/// as the application. The database itself is never created or migrated here.
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

        Services = services.BuildServiceProvider();
    }

    public IConfiguration Configuration { get; }

    public ServiceProvider Services { get; }

    public void Dispose() => Services.Dispose();
}
