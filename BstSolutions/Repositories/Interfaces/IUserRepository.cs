using BstSolutions.Models;

namespace BstSolutions.Repositories.Interfaces;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
