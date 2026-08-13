using BstSolutions.Common;
using BstSolutions.Services.Interfaces;
using BstSolutions.ViewModels.Employee;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BstSolutions.Controllers;

[Authorize]
public class EmployeeController : Controller
{
    private readonly IEmployeeService _employeeService;

    public EmployeeController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var employees = await _employeeService.GetEmployeesAsync(cancellationToken);
        return View(employees);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateEmployeeViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEmployeeViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _employeeService.CreateAsync(model, cancellationToken);
            TempData["Success"] = "Employee created successfully.";
            return Ok(new
            {
                message = "Employee created successfully.",
                redirectUrl = Url.Action(nameof(Index))
            });
        }
        catch (BusinessException ex)
        {
            return Conflict(new { message = ex.UserMessage });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var employee = await _employeeService.GetByIdAsync(id, cancellationToken);
        if (employee is null)
        {
            return NotFound();
        }

        return View(employee);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditEmployeeViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _employeeService.UpdateAsync(model, cancellationToken);
            TempData["Success"] = "Employee updated successfully.";
            return Ok(new
            {
                message = "Employee updated successfully.",
                redirectUrl = Url.Action(nameof(Index))
            });
        }
        catch (BusinessException ex)
        {
            return Conflict(new { message = ex.UserMessage });
        }
    }
}
