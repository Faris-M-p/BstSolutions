using BstSolutions.Data;
using BstSolutions.Models;
using BstSolutions.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BstSolutions.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly ApplicationDbContext _dbContext;

    public EmployeeRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<Employee>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Implementation will be added in a later step.
        throw new NotImplementedException();
    }

    public Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // Implementation will be added in a later step.
        throw new NotImplementedException();
    }

    public Task<bool> EmailExistsAsync(string email, int? excludeEmployeeId = null, CancellationToken cancellationToken = default)
    {
        // Implementation will be added in a later step.
        throw new NotImplementedException();
    }

    public Task AddAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        // Implementation will be added in a later step.
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        // Implementation will be added in a later step.
        throw new NotImplementedException();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
