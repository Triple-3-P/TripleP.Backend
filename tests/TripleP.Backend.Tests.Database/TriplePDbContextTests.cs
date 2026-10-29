using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TripleP.Backend.Abstractions.Database.Entities;
using TripleP.Backend.Database.Context;
using TripleP.Backend.Database.Identity;

namespace TripleP.Backend.Tests.Database;

public class TriplePDbContextTests : IDisposable
{
    private readonly Startup _startup = new();

    public void Dispose() => _startup.Dispose();

    [Fact]
    public void Migrations_are_applied_to_the_test_database()
    {
        using var scope = _startup.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TriplePDbContext>();

        var appliedMigrations = context.Database.GetAppliedMigrations();

        Assert.Contains(appliedMigrations, migration => migration.EndsWith("InitialCreate", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(typeof(User), "Users")]
    [InlineData(typeof(ApplicationUser), "AspNetUsers")]
    [InlineData(typeof(IdentityRole), "AspNetRoles")]
    [InlineData(typeof(IdentityUserClaim<string>), "AspNetUserClaims")]
    [InlineData(typeof(IdentityUserLogin<string>), "AspNetUserLogins")]
    [InlineData(typeof(IdentityUserRole<string>), "AspNetUserRoles")]
    [InlineData(typeof(IdentityUserToken<string>), "AspNetUserTokens")]
    public void Entity_is_mapped_to_expected_table(Type entityType, string tableName)
    {
        using var scope = _startup.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TriplePDbContext>();

        var entity = context.Model.FindEntityType(entityType);

        Assert.NotNull(entity);
        Assert.Equal(tableName, entity.GetTableName());
    }

    [Fact]
    public void User_entity_is_exposed_by_the_context()
    {
        using var scope = _startup.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TriplePDbContext>();

        var entity = context.Model.FindEntityType(typeof(User));

        Assert.NotNull(entity);
        Assert.Equal(nameof(User.Id), entity.FindPrimaryKey()!.Properties[0].Name);
    }

    [Fact]
    public void ApplicationUser_exposes_identity_properties()
    {
        using var scope = _startup.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TriplePDbContext>();

        var entity = context.Model.FindEntityType(typeof(ApplicationUser));

        Assert.NotNull(entity);
        Assert.Equal("Id", entity.FindPrimaryKey()!.Properties[0].Name);
        Assert.NotNull(entity.FindProperty(nameof(ApplicationUser.UserName)));
        Assert.NotNull(entity.FindProperty(nameof(ApplicationUser.Email)));
        Assert.NotNull(entity.FindProperty(nameof(ApplicationUser.PasswordHash)));
    }

    [Fact]
    public void ApplicationUser_has_unique_index_on_normalized_user_name()
    {
        using var scope = _startup.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TriplePDbContext>();

        var entity = context.Model.FindEntityType(typeof(ApplicationUser));

        Assert.NotNull(entity);
        Assert.Contains(
            entity.GetIndexes(),
            index => index.IsUnique
                && index.Properties.Count == 1
                && index.Properties[0].Name == nameof(ApplicationUser.NormalizedUserName));
    }
}
