using BstSolutions.Repositories.Interfaces;
using BstSolutions.Services.Interfaces;
using BstSolutions.ViewModels.Dashboard;

namespace BstSolutions.Services;

public class DashboardService : IDashboardService
{
    private readonly ITaskRepository _taskRepository;

    public DashboardService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public Task<DashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        // Business logic will be implemented in a later step.
        throw new NotImplementedException();
    }
}
