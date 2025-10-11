using Microsoft.AspNetCore.Mvc;

namespace Sistema_Gestion_Llaves.Controllers
{
    public class DocenteDashboardController : Controller
    {
        public IActionResult Index()
        {
            // Verificar sesión y rol
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioEmail")) ||
                HttpContext.Session.GetString("UsuarioRol")?.ToLower() != "docente")
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Email = HttpContext.Session.GetString("UsuarioEmail");
            ViewBag.Rol = HttpContext.Session.GetString("UsuarioRol");

            return View();
        }
    }
}
