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
        return _dbContext.Employees
            .AsNoTracking()
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync(cancellationToken);
    }

    public Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Employees
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public Task<bool> EmailExistsAsync(string email, int? excludeEmployeeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Employees.AsNoTracking()
            .Where(e => e.Email == email);

        if (excludeEmployeeId.HasValue)
        {
            query = query.Where(e => e.Id != excludeEmployeeId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        await _dbContext.Employees.AddAsync(employee, cancellationToken);
    }

    public Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        _dbContext.Employees.Update(employee);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
