using GestionLlaves.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionLlaves.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string contrasenia)
        {
            try
            {
                // Validar que los campos no estén vacíos
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(contrasenia))
                {
                    ViewBag.Mensaje = "Por favor, complete todos los campos";
                    return View();
                }

                // Buscar el usuario por email
                var usuario = await _context.Usuario
                    .Include(u => u.Persona)
                    .FirstOrDefaultAsync(u => u.Email == email && u.Estado == true);

                if (usuario == null)
                {
                    ViewBag.Mensaje = "Usuario o contraseña incorrectos";
                    return View();
                }

                // Verificar la contraseña (asumiendo que usas hash)
                byte[] contraseniaHash = HashPassword(contrasenia);

                if (!usuario.Contrasenia.SequenceEqual(contraseniaHash))
                {
                    ViewBag.Mensaje = "Usuario o contraseña incorrectos";
                    return View();
                }

                // Si es primera vez, obligar a cambiar contraseña
                if (usuario.FechaUltimaConexion == null)
                {
                    return RedirectToAction("CambiarPasswordPrimeraVez", new { email = usuario.Email });
                }
                else
                {
                    // Actualizar última conexión
                    usuario.FechaUltimaConexion = DateTime.Now;
                    await _context.SaveChangesAsync();
                }

                // Crear sesión
                HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
                HttpContext.Session.SetString("UsuarioNombre",
                    $"{usuario.Persona?.Nombres ?? ""} {usuario.Persona?.PrimerApellido ?? ""}");
                HttpContext.Session.SetString("UsuarioRol", usuario.Rol ?? "");
                HttpContext.Session.SetString("UsuarioEmail", usuario.Email ?? "");

                // Redirigir según el rol
                if (usuario.Rol == "ADMIN")
                {
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                }
                else if (usuario.Rol == "DOCENTE")
                {
                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Mensaje = "Rol no válido";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = "Error al iniciar sesión. Intente nuevamente.";
                // Log del error: _logger.LogError(ex, "Error en Login");
                return View();
            }
        }

        // Método helper para hashear contraseña con SHA256
        private byte[] HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                return sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }


        //public IActionResult ForgotPassword()
        //{

        //}

        //public IActionResult ResetPassword()
        //{

        //}

        //public IActionResult ChangePassword()
        //{

        //}

        //public IActionResult Profile()
        //{

        //}
    }
}
