using BstSolutions.ViewModels.Authentication;

namespace BstSolutions.Services.Interfaces;

public interface IAuthenticationService
{
    /// <summary>
    /// Validates email/password against ApplicationUsers.
    /// Returns user info without PasswordHash, or null when authentication fails.
    /// </summary>
    Task<AuthenticatedUserInfo?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default);
}
