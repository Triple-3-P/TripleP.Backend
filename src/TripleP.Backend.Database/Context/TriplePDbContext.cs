using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TripleP.Backend.Abstractions.Database.Entities;
using TripleP.Backend.Database.Identity;

namespace TripleP.Backend.Database.Context;

public class TriplePDbContext : IdentityDbContext<ApplicationUser>
{
    public TriplePDbContext(DbContextOptions<TriplePDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configured here because IdentityDbContext already owns the "Users" DbSet property.
        builder.Entity<User>().ToTable("Users");
    }
}
