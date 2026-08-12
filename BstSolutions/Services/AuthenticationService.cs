using BstSolutions.Common;
using BstSolutions.Models;
using BstSolutions.Repositories.Interfaces;
using BstSolutions.Services.Interfaces;
using BstSolutions.ViewModels.Authentication;
using Microsoft.AspNetCore.Identity;

namespace BstSolutions.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly PasswordHasher<ApplicationUser> _passwordHasher;

    public AuthenticationService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = new PasswordHasher<ApplicationUser>();
    }

    public async Task<AuthenticatedUserInfo> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
        {
            throw new UnauthorizedException(
                "Invalid email or password.",
                "INVALID_CREDENTIALS");
        }

        var normalizedEmail = email.Trim();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedException(
                "Invalid email or password.",
                "INVALID_CREDENTIALS");
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (verification == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedException(
                "Invalid email or password.",
                "INVALID_CREDENTIALS");
        }

        return new AuthenticatedUserInfo
        {
            Id = user.ID_ApplicationUser,
            Email = user.Email,
            Role = user.Role
        };
    }
}
