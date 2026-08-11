using BstSolutions.Common.Enums;
using BstSolutions.Repositories.Interfaces;
using BstSolutions.Services.Interfaces;
using BstSolutions.ViewModels.Dashboard;
using BstSolutions.ViewModels.Task;
using Microsoft.EntityFrameworkCore;

namespace BstSolutions.Services;

public class DashboardService : IDashboardService
{
    private readonly ITaskRepository _taskRepository;

    public DashboardService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<DashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var query = _taskRepository.Query().AsNoTracking();

        var total = await query.CountAsync(cancellationToken);
        var newCount = await query.CountAsync(t => t.Status == WorkTaskStatus.New, cancellationToken);
        var inProgress = await query.CountAsync(t => t.Status == WorkTaskStatus.InProgress, cancellationToken);
        var completed = await query.CountAsync(t => t.Status == WorkTaskStatus.Completed, cancellationToken);
        var overdue = await query.CountAsync(t =>
            t.DueDate < today &&
            t.Status != WorkTaskStatus.Completed &&
            t.Status != WorkTaskStatus.Cancelled, cancellationToken);

        var upcoming = await query
            .Include(t => t.Employee)
            .Where(t =>
                t.DueDate >= today &&
                t.Status != WorkTaskStatus.Completed &&
                t.Status != WorkTaskStatus.Cancelled)
            .OrderBy(t => t.DueDate)
            .Take(5)
            .Select(t => new TaskListItemViewModel
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
            })
            .ToListAsync(cancellationToken);

        return new DashboardViewModel
        {
            TotalTasks = total,
            NewTasks = newCount,
            InProgressTasks = inProgress,
            CompletedTasks = completed,
            OverdueTasks = overdue,
            UpcomingTasks = upcoming
        };
    }
}
