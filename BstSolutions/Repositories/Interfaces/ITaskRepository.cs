using BstSolutions.Models;

namespace BstSolutions.Repositories.Interfaces;

public interface ITaskRepository
{
    /// <summary>
    /// Returns an IQueryable for database-level filtering and sorting in the service layer.
    /// </summary>
    IQueryable<WorkTask> Query();

    Task<WorkTask?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(WorkTask workTask, CancellationToken cancellationToken = default);

    Task UpdateAsync(WorkTask workTask, CancellationToken cancellationToken = default);

    Task DeleteAsync(WorkTask workTask, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
