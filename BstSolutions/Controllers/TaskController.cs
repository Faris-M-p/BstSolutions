using BstSolutions.Services.Interfaces;
using BstSolutions.ViewModels.Task;
using Microsoft.AspNetCore.Mvc;

namespace BstSolutions.Controllers;

public class TaskController : Controller
{
    private readonly ITaskService _taskService;

    public TaskController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    public IActionResult Index(TaskFilterViewModel filter)
    {
        // CRUD / filtering implementation will be added in a later step.
        return View(new TaskListViewModel { Filter = filter });
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateTaskViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CreateTaskViewModel model)
    {
        // CRUD implementation will be added in a later step.
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        // CRUD implementation will be added in a later step.
        return View(new EditTaskViewModel { Id = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(EditTaskViewModel model)
    {
        // CRUD implementation will be added in a later step.
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        // Details implementation will be added in a later step.
        return View(new EditTaskViewModel { Id = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        // Delete implementation will be added in a later step.
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// AJAX endpoint skeleton for marking a task completed without a full page reload.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Complete(int id)
    {
        // AJAX complete implementation will be added in a later step.
        return Json(new { success = false, message = "Not implemented yet." });
    }
}
