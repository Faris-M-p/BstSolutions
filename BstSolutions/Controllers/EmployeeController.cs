using BstSolutions.Services.Interfaces;
using BstSolutions.ViewModels.Employee;
using Microsoft.AspNetCore.Mvc;

namespace BstSolutions.Controllers;

public class EmployeeController : Controller
{
    private readonly IEmployeeService _employeeService;

    public EmployeeController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        // CRUD implementation will be added in a later step.
        return View();
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateEmployeeViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CreateEmployeeViewModel model)
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
        return View(new EditEmployeeViewModel { Id = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(EditEmployeeViewModel model)
    {
        // CRUD implementation will be added in a later step.
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }
}
