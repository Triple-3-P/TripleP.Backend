namespace TripleP.Backend.Tests.Database;

[CollectionDefinition(Name)]
public sealed class PostgresDatabaseCollection : ICollectionFixture<PostgresDatabaseFixture>
{
    public const string Name = "PostgreSQL";
}
