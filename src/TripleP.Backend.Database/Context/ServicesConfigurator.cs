using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TripleP.Backend.Database.Context;

internal static class ServicesConfigurator
{
    public const string ConnectionStringName = "PostgresConnection";

    public static void Configure(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' is missing. " +
                $"Set 'ConnectionStrings:{ConnectionStringName}' through configuration, user secrets or environment variables.");
        }

        services.AddDbContext<TriplePDbContext>(options => options.UseNpgsql(connectionString));
    }
}
