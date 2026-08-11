namespace BstSolutions.ViewModels.Task;

public class TaskListViewModel
{
    public IReadOnlyList<TaskListItemViewModel> Tasks { get; set; } = Array.Empty<TaskListItemViewModel>();

    public TaskFilterViewModel Filter { get; set; } = new();
}
