using BstSolutions.ViewModels.Task;

namespace BstSolutions.ViewModels.Dashboard;

public class DashboardViewModel
{
    public int TotalTasks { get; set; }

    public int NewTasks { get; set; }

    public int InProgressTasks { get; set; }

    public int CompletedTasks { get; set; }

    public int OverdueTasks { get; set; }

    public IReadOnlyList<TaskListItemViewModel> UpcomingTasks { get; set; } = Array.Empty<TaskListItemViewModel>();
}
