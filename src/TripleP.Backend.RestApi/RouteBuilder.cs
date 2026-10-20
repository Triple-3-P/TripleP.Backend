namespace TripleP.Backend.RestApi
{
    public static class RouteBuilder
    {
        public static void Register(this IEndpointRouteBuilder routeBuilder)
        {
            Api.RouteBuilder.Register(routeBuilder);
        }
    }
}
namespace Microsoft.AspNetCore.Builder
{
    using TripleP.Backend.RestApi;

    public static class TriplePEndpointBuilderExtensions
    {
        public static IEndpointRouteBuilder UseTriplePEndpoints(this IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.Register();
            return routeBuilder;
        }
    }
}