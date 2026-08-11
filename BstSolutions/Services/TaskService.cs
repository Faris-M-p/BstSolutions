using BstSolutions.Common;
using BstSolutions.Common.Enums;
using BstSolutions.Models;
using BstSolutions.Repositories.Interfaces;
using BstSolutions.Services.Interfaces;
using BstSolutions.ViewModels.Task;
using Microsoft.EntityFrameworkCore;

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

    public async Task<TaskListViewModel> GetTasksAsync(TaskFilterViewModel filter, CancellationToken cancellationToken = default)
    {
        filter ??= new TaskFilterViewModel();

        var query = _taskRepository.Query()
            .AsNoTracking()
            .Include(t => t.Employee)
            .AsQueryable();

        if (filter.EmployeeId.HasValue)
        {
            query = query.Where(t => t.EmployeeId == filter.EmployeeId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(t => t.Status == filter.Status.Value);
        }

        if (filter.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == filter.Priority.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(t =>
                t.Title.Contains(search) ||
                (t.Description != null && t.Description.Contains(search)) ||
                t.Employee.FirstName.Contains(search) ||
                t.Employee.LastName.Contains(search));
        }

        query = ApplySort(query, filter.SortBy, filter.SortDirection);

        var tasks = await query.Select(t => new TaskListItemViewModel
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            EmployeeId = t.EmployeeId,
            EmployeeName = t.Employee.FirstName + " " + t.Employee.LastName,
            Priority = t.Priority,
            Status = t.Status,
            DueDate = t.DueDate,
            CreatedDate = t.CreatedDate,
            CompletedDate = t.CompletedDate
        }).ToListAsync(cancellationToken);

        return new TaskListViewModel
        {
            Tasks = tasks,
            Filter = filter
        };
    }

    public async Task<EditTaskViewModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            return null;
        }

        return new EditTaskViewModel
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            EmployeeId = task.EmployeeId,
            Priority = task.Priority,
            Status = task.Status,
            DueDate = task.DueDate,
            RowVersion = task.RowVersion
        };
    }

    public async Task<TaskDetailsViewModel?> GetDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            return null;
        }

        return new TaskDetailsViewModel
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            EmployeeId = task.EmployeeId,
            EmployeeName = $"{task.Employee.FirstName} {task.Employee.LastName}",
            Priority = task.Priority,
            Status = task.Status,
            DueDate = task.DueDate,
            CreatedDate = task.CreatedDate,
            CompletedDate = task.CompletedDate
        };
    }

    public async Task CreateAsync(CreateTaskViewModel model, CancellationToken cancellationToken = default)
    {
        if (model.EmployeeId <= 0)
        {
            throw new BusinessException("A task cannot be created without an assigned employee.");
        }

        var employee = await _employeeRepository.GetByIdAsync(model.EmployeeId, cancellationToken)
            ?? throw new BusinessException("Assigned employee was not found.");

        if (!employee.IsActive)
        {
            throw new BusinessException("Only active employees can be assigned to new tasks.");
        }

        if (model.DueDate.Date < DateTime.Today)
        {
            throw new BusinessException("Due date cannot be earlier than today's date.");
        }

        var task = new WorkTask
        {
            Title = model.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
            EmployeeId = model.EmployeeId,
            Priority = model.Priority,
            Status = WorkTaskStatus.New,
            DueDate = model.DueDate.Date,
            CreatedDate = DateTime.UtcNow,
            CompletedDate = null
        };

        await _taskRepository.AddAsync(task, cancellationToken);
        await _taskRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(EditTaskViewModel model, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(model.Id, cancellationToken)
            ?? throw new BusinessException("Task not found.");

        if (model.EmployeeId <= 0)
        {
            throw new BusinessException("A task must have an assigned employee.");
        }

        var employee = await _employeeRepository.GetByIdAsync(model.EmployeeId, cancellationToken)
            ?? throw new BusinessException("Assigned employee was not found.");

        // Reassignment to a new employee still requires an active employee.
        if (task.EmployeeId != model.EmployeeId && !employee.IsActive)
        {
            throw new BusinessException("Only active employees can be assigned to tasks.");
        }

        task.Title = model.Title.Trim();
        task.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
        task.EmployeeId = model.EmployeeId;
        task.Priority = model.Priority;
        task.DueDate = model.DueDate.Date;

        ApplyStatusChange(task, model.Status);

        _taskRepository.SetOriginalRowVersion(task, model.RowVersion);

        try
        {
            await _taskRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new BusinessException("This task was modified by another user. Please reload and try again.");
        }
    }

    public async Task CompleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new BusinessException("Task not found.");

        if (task.Status == WorkTaskStatus.Completed)
        {
            throw new BusinessException("Task is already completed.");
        }

        ApplyStatusChange(task, WorkTaskStatus.Completed);
        await _taskRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new BusinessException("Task not found.");

        if (task.Status == WorkTaskStatus.Completed)
        {
            throw new BusinessException("A completed task cannot be deleted.");
        }

        await _taskRepository.DeleteAsync(task, cancellationToken);
        await _taskRepository.SaveChangesAsync(cancellationToken);
    }

    private static void ApplyStatusChange(WorkTask task, WorkTaskStatus newStatus)
    {
        task.Status = newStatus;

        if (newStatus == WorkTaskStatus.Completed)
        {
            task.CompletedDate = DateTime.UtcNow;
        }
        else
        {
            task.CompletedDate = null;
        }
    }

    private static IQueryable<WorkTask> ApplySort(IQueryable<WorkTask> query, string? sortBy, string? sortDirection)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return (sortBy?.ToLowerInvariant()) switch
        {
            "duedate" => descending
                ? query.OrderByDescending(t => t.DueDate)
                : query.OrderBy(t => t.DueDate),
            "priority" => descending
                ? query.OrderByDescending(t => t.Priority)
                : query.OrderBy(t => t.Priority),
            "employee" => descending
                ? query.OrderByDescending(t => t.Employee.FirstName).ThenByDescending(t => t.Employee.LastName)
                : query.OrderBy(t => t.Employee.FirstName).ThenBy(t => t.Employee.LastName),
            _ => descending
                ? query.OrderByDescending(t => t.CreatedDate)
                : query.OrderBy(t => t.CreatedDate)
        };
    }
}
