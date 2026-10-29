namespace TripleP.Backend.RestApi.Api.Health;

public static class RouteBuilder
{
    public static void Register(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapGet("/health", Endpoint.Execute)
            .WithName("Health")
            .WithTags("Health");
    }
}
