namespace BstSolutions.ViewModels.Authentication;

/// <summary>
/// Authenticated user info returned to AccountController (no PasswordHash).
/// </summary>
public class AuthenticatedUserInfo
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
}
