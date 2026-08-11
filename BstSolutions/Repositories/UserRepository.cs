using BstSolutions.Data;
using BstSolutions.Models;
using BstSolutions.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BstSolutions.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return _dbContext.ApplicationUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }
}
