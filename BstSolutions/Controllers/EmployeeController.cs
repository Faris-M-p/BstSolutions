using BstSolutions.Common;
using BstSolutions.Services.Interfaces;
using BstSolutions.ViewModels.Employee;
using Microsoft.AspNetCore.Mvc;

namespace BstSolutions.Controllers;

// [Authorize] — enable when authentication is added (Task 13).
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
            return View(model);
        }

        try
        {
            await _employeeService.CreateAsync(model, cancellationToken);
            TempData["Success"] = "Employee created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
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
            return View(model);
        }

        try
        {
            await _employeeService.UpdateAsync(model, cancellationToken);
            TempData["Success"] = "Employee updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }
}
