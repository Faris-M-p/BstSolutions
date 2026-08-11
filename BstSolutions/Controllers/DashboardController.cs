using BstSolutions.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BstSolutions.Controllers;

public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        // Dashboard implementation will be added in a later step.
        return View();
    }
}
