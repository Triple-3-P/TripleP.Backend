namespace TripleP.Backend.RestApi.Api.Health;

public static class Endpoint
{
    public static IResult Execute()
    {
        return Results.Ok(new { status = "Healthy" });
    }
}
