using Microsoft.Extensions.DependencyInjection;

namespace TripleP.Backend.Logic
{
    internal static class ServicesConfigurator
    {
        public static void Configure(IServiceCollection services)
        {
            Services.ServicesConfigurator.Configure(services);
        }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    using TripleP.Backend.Logic;

    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddTriplePLogic(this IServiceCollection services)
        {
            ServicesConfigurator.Configure(services);
            return services;
        }
    }
}