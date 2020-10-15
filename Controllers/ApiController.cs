using Microsoft.AspNetCore.Mvc;

namespace WeatherLink.Controllers
{
    public class ApiController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }

        // Get
        public IActionResult Estaciones()
        {
            return View();
        }
    }
}