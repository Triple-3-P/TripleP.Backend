namespace TripleP.Backend.RestApi.Api;

public static class RouteBuilder
{
    public static void Register(this IEndpointRouteBuilder routeBuilder)
    {
        var group = routeBuilder.MapGroup("api");

        Health.RouteBuilder.Register(group);
    }
}
