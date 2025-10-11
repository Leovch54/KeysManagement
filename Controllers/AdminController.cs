using Microsoft.AspNetCore.Mvc;

namespace GestionLlaves.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
