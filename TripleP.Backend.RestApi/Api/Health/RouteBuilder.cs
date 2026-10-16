namespace TripleP.Backend.RestApi.Api.Health;

public static class RouteBuilder
{
    public static IEndpointRouteBuilder MapHealthRoutes(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapGet("/health", Endpoint.Execute)
            .WithName("Health");

        return routeBuilder;
    }
}
