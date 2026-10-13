using TripleP.Backend.RestApi.Api.Health;

namespace TripleP.Backend.RestApi.Api;

public static class RouteBuilder
{
    public static IEndpointRouteBuilder MapApiRoutes(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapHealthRoutes();

        return routeBuilder;
    }
}
