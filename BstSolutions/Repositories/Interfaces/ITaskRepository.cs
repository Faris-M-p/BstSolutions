using BstSolutions.Models;

namespace BstSolutions.Repositories.Interfaces;

public interface ITaskRepository
{
    IQueryable<WorkTask> Query();

    Task<WorkTask?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(WorkTask workTask, CancellationToken cancellationToken = default);

    Task UpdateAsync(WorkTask workTask, CancellationToken cancellationToken = default);

    void SetOriginalRowVersion(WorkTask workTask, byte[] rowVersion);

    Task DeleteAsync(WorkTask workTask, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
