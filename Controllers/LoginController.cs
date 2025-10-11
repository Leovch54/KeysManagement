using Microsoft.AspNetCore.Mvc;
using Sistema_Gestion_Llaves.Models;
using System.Net;
using System.Net.Mail;

namespace Sistema_Gestion_Llaves.Controllers
{
    public class LoginController : Controller
    {
        private readonly SistemaGestionLlavesContext _context;

        public LoginController(SistemaGestionLlavesContext context)
        {
            _context = context;
        }

        // GET: Login
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public IActionResult Index(string email, string pin)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == email && u.Pin == pin);

            if (usuario != null)
            {
                // Guardar info en sesión
                HttpContext.Session.SetString("UsuarioEmail", usuario.Email);
                HttpContext.Session.SetString("UsuarioRol", usuario.Rol);

                // Actualizar fecha de última conexión
                usuario.FechaUltimaConexion = DateTime.Now;
                _context.SaveChanges();

                // Si es primera vez, obligar a cambiar contraseña
                if (usuario.PrimeraVez)
                {
                    return RedirectToAction("CambiarPasswordPrimeraVez", new { email = usuario.Email });
                }

                // Redirigir según rol
                if (usuario.Rol.ToLower() == "admin")
                    return RedirectToAction("Index", "AdminDashboard");
                else if (usuario.Rol.ToLower() == "docente")
                    return RedirectToAction("Index", "DocenteDashboard");
                else
                {
                    ViewBag.Mensaje = $"Usuario existe pero rol desconocido: {usuario.Rol}";
                    return View();
                }
            }
            else
            {
                ViewBag.Mensaje = "Usuario inexistente";
                return View();
            }
        }

        // GET: Cambiar contraseña primera vez
        [HttpGet]
        public IActionResult CambiarPasswordPrimeraVez(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        // POST: Cambiar contraseña primera vez
        [HttpPost]
        public IActionResult CambiarPasswordPrimeraVez(string email, string contraseñaActual, string nuevaPassword, string confirmarPassword)
        {
            // Buscar usuario por email
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == email);
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no encontrado.";
                ViewBag.Email = email;
                return View();
            }

            // Verificar contraseña actual
            if (usuario.Pin != contraseñaActual) // ⚠️ En producción, compara el hash
            {
                ViewBag.Mensaje = "La contraseña actual es incorrecta.";
                ViewBag.Email = email;
                return View();
            }

            // Verificar que la nueva contraseña coincida
            if (nuevaPassword != confirmarPassword)
            {
                ViewBag.Mensaje = "Las contraseñas no coinciden.";
                ViewBag.Email = email;
                return View();
            }

            // Actualizar la contraseña y marcar que ya no es primera vez
            usuario.Pin = nuevaPassword; // ⚠️ En producción, hashea la contraseña
            usuario.PrimeraVez = false;
            _context.SaveChanges();

            ViewBag.Mensaje = "Contraseña actualizada correctamente. Ahora puedes iniciar sesión.";
            return RedirectToAction("Index", "Login");
        }


        // GET: ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                ViewBag.Mensaje = "Enlace inválido.";
                return View("Error");
            }

            ViewBag.Email = email;
            ViewBag.Token = token;

            return View();
        }

        // POST: ResetPassword
        [HttpPost]
        public IActionResult ResetPassword(string email, string token, string nuevaPassword, string confirmarPassword)
        {
            if (nuevaPassword != confirmarPassword)
            {
                ViewBag.Mensaje = "Las contraseñas no coinciden.";
                ViewBag.Email = email;
                ViewBag.Token = token;
                return View();
            }

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == email);
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no encontrado.";
                return View();
            }

            usuario.Pin = nuevaPassword; // ⚠️ En producción, siempre hashea la contraseña
            _context.SaveChanges();

            ViewBag.Mensaje = "Contraseña actualizada correctamente. Ahora puedes iniciar sesión.";
            return RedirectToAction("Index");
        }

        // GET: Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        // GET: Recuperar contraseña
        [HttpGet]
        public IActionResult RecuperarPassword()
        {
            return View();
        }

        // POST: Recuperar contraseña
        [HttpPost]
        public IActionResult RecuperarPassword(string email)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == email);

            if (usuario != null)
            {
                var token = Guid.NewGuid().ToString();
                var resetUrl = $"http://localhost:5173/Login/ResetPassword?token={token}&email={email}";

                var body = $@"
                    <!DOCTYPE html>
                    <html lang='es'>
                    <head>
                        <meta charset='UTF-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                        <title>Recuperación de Contraseña</title>
                    </head>
                    <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; margin:0; padding:0;'>
                        <table width='100%' cellpadding='0' cellspacing='0'>
                            <tr>
                                <td align='center'>
                                    <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; padding: 30px; border-radius: 10px;'>
                                        <tr>
                                            <td align='center'>
                                              <img src='https://localhost:7200/images/logo.png' alt='Logo' width='120' style='margin-bottom:20px;' />
                                                <h2 style='color:#72103A;'>Recuperación de Contraseña</h2>
                                                <p>Hola <strong>{usuario.Email}</strong>,</p>
                                                <p>Haz clic en el siguiente botón para restablecer tu contraseña:</p>
                                                <p style='text-align:center; margin: 30px 0;'>
                                                    <a href='{resetUrl}' style='background-color:#72103A; color:#fff; padding:15px 25px; text-decoration:none; border-radius:5px; display:inline-block;'>Restablecer Contraseña</a>
                                                </p>
                                                <p>Si no solicitaste este cambio, puedes ignorar este correo.</p>
                                                <hr style='border:none; border-top:1px solid #ddd; margin:30px 0;' />
                                                <p style='font-size:12px; color:#666;'>Universidad del Valle - Bolivia</p>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </body>
                    </html>";

                try
                {
                    var message = new MailMessage();
                    message.From = new MailAddress("alberth22291@gmail.com", "Sistema Gestión Llaves");
                    message.To.Add(email);
                    message.Subject = "Recuperación de Contraseña";
                    message.Body = body;
                    message.IsBodyHtml = true;

                    using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential("alberth22291@gmail.com", "tuAppPasswordGmail");
                        smtp.EnableSsl = true;
                        smtp.Send(message);
                    }

                    ViewBag.Mensaje = "Correo enviado correctamente. Revisa tu bandeja de entrada.";
                }
                catch (Exception ex)
                {
                    ViewBag.Mensaje = "Error al enviar el correo: " + ex.Message;
                }
            }
            else
            {
                ViewBag.Mensaje = "Este correo no está vinculado a ninguna cuenta.";
            }

            return View();
        }
    }
}
