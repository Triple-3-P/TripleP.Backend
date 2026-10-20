using Microsoft.Extensions.DependencyInjection;

namespace TripleP.Backend.Database
{
    internal static class ServicesConfigurator
    {
        public static void Configure(IServiceCollection services)
        {
            Repositories.ServicesConfigurator.Configure(services);
        }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    using TripleP.Backend.Database;

    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddTriplePDatabase(this IServiceCollection services)
        {
            ServicesConfigurator.Configure(services);
            return services;
        }
    }
}