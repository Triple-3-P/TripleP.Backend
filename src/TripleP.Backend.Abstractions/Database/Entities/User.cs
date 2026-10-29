namespace TripleP.Backend.Abstractions.Database.Entities;

/// <summary>
/// Application user domain entity. Dependency-free on purpose: the ASP.NET Identity
/// user lives in the Database layer and additional fields will be added here later.
/// </summary>
public class User
{
    public Guid Id { get; set; }
}
