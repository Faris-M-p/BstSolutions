namespace BstSolutions.Models;

public class ApplicationUser
{
    public int ID_ApplicationUser { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public string Role { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }
}
