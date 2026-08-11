using BstSolutions.Data;
using BstSolutions.Models;
using BstSolutions.Repositories.Interfaces;

namespace BstSolutions.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly ApplicationDbContext _dbContext;

    public TaskRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<WorkTask> Query()
    {
        return _dbContext.WorkTasks.AsQueryable();
    }

    public Task<WorkTask?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // Implementation will be added in a later step.
        throw new NotImplementedException();
    }

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        // Implementation will be added in a later step.
        throw new NotImplementedException();
    }

    public Task AddAsync(WorkTask workTask, CancellationToken cancellationToken = default)
    {
        // Implementation will be added in a later step.
        throw new NotImplementedException();
    }

    public Task UpdateAsync(WorkTask workTask, CancellationToken cancellationToken = default)
    {
        // Implementation will be added in a later step.
        throw new NotImplementedException();
    }

    public Task DeleteAsync(WorkTask workTask, CancellationToken cancellationToken = default)
    {
        // Implementation will be added in a later step.
        throw new NotImplementedException();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
