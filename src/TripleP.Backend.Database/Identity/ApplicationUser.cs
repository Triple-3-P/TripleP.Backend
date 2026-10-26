using Microsoft.AspNetCore.Identity;

namespace TripleP.Backend.Database.Identity;

/// <summary>
/// ASP.NET Identity user stored in PostgreSQL. It is kept separate from the
/// dependency-free <c>User</c> entity in Abstractions; extra Identity fields can be
/// added here without leaking Identity types into the Abstractions project.
/// </summary>
public class ApplicationUser : IdentityUser
{
}
