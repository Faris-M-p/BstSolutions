using BstSolutions.Common;
using BstSolutions.Common.Responses;
using BstSolutions.Services.Interfaces;
using BstSolutions.ViewModels.Task;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BstSolutions.Controllers;

[Authorize]
public class TaskController : Controller
{
    private readonly ITaskService _taskService;
    private readonly IEmployeeService _employeeService;

    public TaskController(ITaskService taskService, IEmployeeService employeeService)
    {
        _taskService = taskService;
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(TaskFilterViewModel filter, CancellationToken cancellationToken)
    {
        var model = await _taskService.GetTasksAsync(filter, cancellationToken);
        await PopulateFilterLookupsAsync(filter.EmployeeId, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateEmployeeOptionsAsync(activeOnly: true, cancellationToken);
        return View(new CreateTaskViewModel
        {
            DueDate = DateTime.Today,
            Priority = Common.Enums.Priority.Medium
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTaskViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateEmployeeOptionsAsync(activeOnly: true, cancellationToken);
            return View(model);
        }

        try
        {
            await _taskService.CreateAsync(model, cancellationToken);
            TempData["Success"] = "Task created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.UserMessage);
            await PopulateEmployeeOptionsAsync(activeOnly: true, cancellationToken);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var model = await _taskService.GetByIdAsync(id, cancellationToken);
        if (model is null)
        {
            return NotFound();
        }

        await PopulateEmployeeOptionsAsync(activeOnly: false, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditTaskViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateEmployeeOptionsAsync(activeOnly: false, cancellationToken);
            return View(model);
        }

        try
        {
            await _taskService.UpdateAsync(model, cancellationToken);
            TempData["Success"] = "Task updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.UserMessage);
            await PopulateEmployeeOptionsAsync(activeOnly: false, cancellationToken);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var model = await _taskService.GetDetailsAsync(id, cancellationToken);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _taskService.DeleteAsync(id, cancellationToken);
            TempData["Success"] = "Task deleted successfully.";
        }
        catch (BusinessException ex)
        {
            TempData["Error"] = ex.UserMessage;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _taskService.CompleteAsync(id, cancellationToken);
            return Ok(ApiResponse.Ok("Task completed successfully."));
        }
        catch (BusinessException ex)
        {
            var statusCode = ex.ErrorCode switch
            {
                "TASK_NOT_FOUND" => StatusCodes.Status404NotFound,
                "CONCURRENCY_CONFLICT" => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status400BadRequest
            };

            return StatusCode(statusCode, ApiResponse.Fail(ex.UserMessage, ex.ErrorCode));
        }
    }

    private async Task PopulateEmployeeOptionsAsync(bool activeOnly, CancellationToken cancellationToken)
    {
        var employees = activeOnly
            ? await _employeeService.GetActiveEmployeesAsync(cancellationToken)
            : await _employeeService.GetEmployeesAsync(cancellationToken);

        ViewBag.Employees = new SelectList(
            employees.Select(e => new { e.Id, Name = e.FullName }),
            "Id",
            "Name");
    }

    private async Task PopulateFilterLookupsAsync(int? selectedEmployeeId, CancellationToken cancellationToken)
    {
        var employees = await _employeeService.GetEmployeesAsync(cancellationToken);
        ViewBag.Employees = new SelectList(
            employees.Select(e => new { e.Id, Name = e.FullName }),
            "Id",
            "Name",
            selectedEmployeeId);
    }
}
