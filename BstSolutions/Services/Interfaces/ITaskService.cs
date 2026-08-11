using BstSolutions.ViewModels.Task;

namespace BstSolutions.Services.Interfaces;

public interface ITaskService
{
    Task<TaskListViewModel> GetTasksAsync(TaskFilterViewModel filter, CancellationToken cancellationToken = default);

    Task<EditTaskViewModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task CreateAsync(CreateTaskViewModel model, CancellationToken cancellationToken = default);

    Task UpdateAsync(EditTaskViewModel model, CancellationToken cancellationToken = default);

    Task CompleteAsync(int id, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
