using Microsoft.AspNetCore.Identity;
using TripleP.Backend.Database.Identity;

namespace TripleP.Backend.Web.Infrastructure.Email;

/// <summary>
/// Placeholder sender for the standard Identity API endpoints until a real email provider is
/// configured. It intentionally delivers nothing.
/// </summary>
public sealed class NoOpEmailSender : IEmailSender<ApplicationUser>
{
    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        Task.CompletedTask;

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        Task.CompletedTask;

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
        Task.CompletedTask;
}
