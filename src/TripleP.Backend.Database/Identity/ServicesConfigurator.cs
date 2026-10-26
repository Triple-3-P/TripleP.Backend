using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TripleP.Backend.Database.Context;

namespace TripleP.Backend.Database.Identity;

internal static class ServicesConfigurator
{
    public static void Configure(IServiceCollection services)
    {
        // Registers Identity together with the token/cookie authentication schemes the
        // standard Identity API endpoints (MapIdentityApi) need.
        services
            .AddIdentityApiEndpoints<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<TriplePDbContext>();

        services.AddTransient<IEmailSender<ApplicationUser>, NoOpEmailSender>();

        // UseAuthorization in the pipeline requires these services; Identity does not register them.
        services.AddAuthorization();
    }
}
