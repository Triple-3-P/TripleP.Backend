using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TripleP.Backend.Database
{
    internal static class ServicesConfigurator
    {
        public static void Configure(IServiceCollection services, IConfiguration configuration)
        {
            Context.ServicesConfigurator.Configure(services, configuration);
            Repositories.ServicesConfigurator.Configure(services);
        }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    using TripleP.Backend.Database;

    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddTriplePDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            ServicesConfigurator.Configure(services, configuration);
            return services;
        }
    }
}
