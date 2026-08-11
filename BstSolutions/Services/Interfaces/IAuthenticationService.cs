using BstSolutions.ViewModels.Authentication;

namespace BstSolutions.Services.Interfaces;

public interface IAuthenticationService
{
    Task<AuthenticatedUserInfo?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default);
}
