using Microsoft.AspNetCore.Mvc;

namespace Loco1.Web.Controllers;

public class RepairController : Controller
    {
    public IActionResult Index()
        {
        return View("_UnderConstruction");
        }
    }
