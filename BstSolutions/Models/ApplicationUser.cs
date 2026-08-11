namespace BstSolutions.Models;

/// <summary>
/// Application login user (not an Employee). Stored in ApplicationUsers.
/// </summary>
public class ApplicationUser
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public string Role { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }
}
