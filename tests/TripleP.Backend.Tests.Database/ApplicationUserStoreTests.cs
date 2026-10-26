using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TripleP.Backend.Database.Identity;

namespace TripleP.Backend.Tests.Database;

[Collection(PostgresDatabaseCollection.Name)]
public class ApplicationUserStoreTests
{
    private readonly PostgresDatabaseFixture _fixture;

    public ApplicationUserStoreTests(PostgresDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task UserManager_creates_and_finds_user_in_postgres()
    {
        using var scope = _fixture.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var userName = $"user-{Guid.NewGuid():N}";
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = $"{userName}@example.com",
        };

        var createResult = await userManager.CreateAsync(user, "Str0ng!Password");

        Assert.True(createResult.Succeeded, string.Join("; ", createResult.Errors.Select(error => error.Description)));

        var found = await userManager.FindByNameAsync(userName);

        Assert.NotNull(found);
        Assert.Equal(user.Id, found.Id);
        Assert.False(string.IsNullOrEmpty(found.PasswordHash));
    }

    [Fact]
    public async Task Duplicate_normalized_user_name_hits_unique_index()
    {
        using var scope = _fixture.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var userName = $"user-{Guid.NewGuid():N}";
        var existing = new ApplicationUser
        {
            UserName = userName,
            Email = $"{userName}@example.com",
        };

        var createResult = await userManager.CreateAsync(existing, "Str0ng!Password");

        Assert.True(createResult.Succeeded, string.Join("; ", createResult.Errors.Select(error => error.Description)));

        var duplicate = new ApplicationUser
        {
            UserName = existing.UserName,
            NormalizedUserName = existing.NormalizedUserName,
            Email = $"duplicate-{userName}@example.com",
            NormalizedEmail = $"duplicate-{userName}@example.com".ToUpperInvariant(),
            PasswordHash = "placeholder-hash",
            SecurityStamp = Guid.NewGuid().ToString(),
        };

        _fixture.Context.Users.Add(duplicate);

        try
        {
            // Bypasses UserManager's duplicate check so the PostgreSQL index itself is exercised.
            var exception = await Assert.ThrowsAsync<DbUpdateException>(
                () => _fixture.Context.SaveChangesAsync());

            var postgresException = Assert.IsType<PostgresException>(exception.InnerException);
            Assert.Equal(PostgresErrorCodes.UniqueViolation, postgresException.SqlState);
        }
        finally
        {
            _fixture.Context.Entry(duplicate).State = EntityState.Detached;
            await userManager.DeleteAsync(existing);
        }
    }
}
