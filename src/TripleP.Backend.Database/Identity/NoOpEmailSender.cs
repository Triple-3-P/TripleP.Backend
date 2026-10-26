using Microsoft.AspNetCore.Identity;

namespace TripleP.Backend.Database.Identity;

/// <summary>
/// Placeholder sender for the standard Identity API endpoints until a real email provider is
/// configured. It intentionally delivers nothing; replace it with a real implementation.
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
