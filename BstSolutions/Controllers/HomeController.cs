using Microsoft.AspNetCore.Mvc;

namespace BstSolutions.Controllers;

/// <summary>
/// Minimal home/error controller used by global exception handling and default navigation.
/// </summary>
public class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    public IActionResult Error()
    {
        return View();
    }
}
