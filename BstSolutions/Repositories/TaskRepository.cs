using BstSolutions.Data;
using BstSolutions.Models;
using BstSolutions.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

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
        return _dbContext.WorkTasks
            .Include(t => t.Employee)
            .FirstOrDefaultAsync(t => t.ID_WorkTask == id, cancellationToken);
    }

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.WorkTasks.AsNoTracking()
            .AnyAsync(t => t.ID_WorkTask == id, cancellationToken);
    }

    public async Task AddAsync(WorkTask workTask, CancellationToken cancellationToken = default)
    {
        await _dbContext.WorkTasks.AddAsync(workTask, cancellationToken);
    }

    public Task UpdateAsync(WorkTask workTask, CancellationToken cancellationToken = default)
    {
        _dbContext.WorkTasks.Update(workTask);
        return Task.CompletedTask;
    }

    public void SetOriginalRowVersion(WorkTask workTask, byte[] rowVersion)
    {
        _dbContext.Entry(workTask).Property(t => t.RowVersion).OriginalValue = rowVersion;
    }

    public Task DeleteAsync(WorkTask workTask, CancellationToken cancellationToken = default)
    {
        _dbContext.WorkTasks.Remove(workTask);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
