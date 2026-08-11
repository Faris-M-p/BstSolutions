using BstSolutions.ViewModels.Dashboard;

namespace BstSolutions.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken = default);
}
