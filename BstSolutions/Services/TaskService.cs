using BstSolutions.Repositories.Interfaces;
using BstSolutions.Services.Interfaces;
using BstSolutions.ViewModels.Task;

namespace BstSolutions.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public TaskService(ITaskRepository taskRepository, IEmployeeRepository employeeRepository)
    {
        _taskRepository = taskRepository;
        _employeeRepository = employeeRepository;
    }

    public Task<TaskListViewModel> GetTasksAsync(TaskFilterViewModel filter, CancellationToken cancellationToken = default)
    {
        // Business logic will be implemented in a later step.
        throw new NotImplementedException();
    }

    public Task<EditTaskViewModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // Business logic will be implemented in a later step.
        throw new NotImplementedException();
    }

    public Task CreateAsync(CreateTaskViewModel model, CancellationToken cancellationToken = default)
    {
        // Business logic will be implemented in a later step.
        throw new NotImplementedException();
    }

    public Task UpdateAsync(EditTaskViewModel model, CancellationToken cancellationToken = default)
    {
        // Business logic will be implemented in a later step.
        throw new NotImplementedException();
    }

    public Task CompleteAsync(int id, CancellationToken cancellationToken = default)
    {
        // Business logic will be implemented in a later step.
        throw new NotImplementedException();
    }

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        // Business logic will be implemented in a later step.
        throw new NotImplementedException();
    }
}
