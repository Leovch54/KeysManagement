using GestionLlaves.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace GestionLlaves.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // ======================================
        // LOGIN
        // ======================================
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
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(contrasenia))
                {
                    ViewBag.Mensaje = "Por favor, complete todos los campos";
                    return View();
                }

                var usuario = await _context.Usuario
                    .Include(u => u.Persona)
                    .FirstOrDefaultAsync(u => u.Email == email && u.Estado == true);

                if (usuario == null)
                {
                    ViewBag.Mensaje = "Usuario o contraseña incorrectos";
                    return View();
                }

                byte[] contraseniaHash = HashPassword(contrasenia);
                if (!usuario.Contrasenia.SequenceEqual(contraseniaHash))
                {
                    ViewBag.Mensaje = "Usuario o contraseña incorrectos";
                    return View();
                }

                // Primera vez
                if (usuario.FechaUltimaConexion == null)
                {
                    return RedirectToAction("CambiarPasswordPrimeraVez", new { email = usuario.Email });
                }

                // Actualizar última conexión
                usuario.FechaUltimaConexion = DateTime.Now;
                await _context.SaveChangesAsync();

                // Crear sesión
                HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
                HttpContext.Session.SetString("UsuarioNombre",
                    $"{usuario.Persona?.Nombres ?? ""} {usuario.Persona?.PrimerApellido ?? ""}");
                HttpContext.Session.SetString("UsuarioRol", usuario.Rol ?? "");
                HttpContext.Session.SetString("UsuarioEmail", usuario.Email ?? "");

                // Redirección según rol
                if (usuario.Rol.ToUpper() == "ADMIN")
                    return RedirectToAction("Index", "Admin");
                else if (usuario.Rol.ToUpper() == "DOCENTE")
                    return RedirectToAction("Index", "Docente");

                ViewBag.Mensaje = "Rol no válido";
                return View();
            }
            catch (Exception)
            {
                ViewBag.Mensaje = "Error al iniciar sesión. Intente nuevamente.";
                return View();
            }
        }

        // ======================================
        // HASH DE CONTRASEÑA
        // ======================================
        private byte[] HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        // ======================================
        // LOGOUT
        // ======================================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ======================================
        // CAMBIAR CONTRASEÑA (PRIMERA VEZ)
        // ======================================
        [HttpGet]
        public IActionResult CambiarPasswordPrimeraVez(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CambiarPasswordPrimeraVez(string email, string nuevaPassword, string confirmarPassword)
        {
            if (nuevaPassword != confirmarPassword)
            {
                ViewBag.Mensaje = "Las contraseñas no coinciden.";
                ViewBag.Email = email;
                return View();
            }

            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no encontrado.";
                return View();
            }

            usuario.Contrasenia = HashPassword(nuevaPassword);
            usuario.FechaUltimaConexion = DateTime.Now;
            await _context.SaveChangesAsync();

            ViewBag.Mensaje = "Contraseña actualizada correctamente. Ahora puede iniciar sesión.";
            return RedirectToAction("Login");
        }

        // ======================================
        // RECUPERAR CONTRASEÑA
        // ======================================
        [HttpGet]
        public IActionResult RecuperarPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RecuperarPassword(string email)
        {
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null)
            {
                ViewBag.Mensaje = "Este correo no está vinculado a ninguna cuenta.";
                return View();
            }

            var token = Guid.NewGuid().ToString();
            var resetUrl = $"{Request.Scheme}://{Request.Host}/Account/ResetPassword?token={token}&email={email}";

            string body = $@"
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
                                            <img src='https://localhost:7081/images/logo.png' alt='Logo' width='120' style='margin-bottom:20px;' />
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

            return View();
        }

        // ======================================
        // RESET PASSWORD (ENLACE DEL CORREO)
        // ======================================
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                ViewBag.Mensaje = "Enlace inválido o expirado.";
                return View("Error");
            }

            ViewBag.Email = email;
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string email, string token, string nuevaPassword, string confirmarPassword)
        {
            if (nuevaPassword != confirmarPassword)
            {
                ViewBag.Mensaje = "Las contraseñas no coinciden.";
                ViewBag.Email = email;
                ViewBag.Token = token;
                return View();
            }

            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no encontrado.";
                return View();
            }

            usuario.Contrasenia = HashPassword(nuevaPassword);
            await _context.SaveChangesAsync();

            ViewBag.Mensaje = "Contraseña actualizada correctamente. Ahora puede iniciar sesión.";
            return RedirectToAction("Login");
        }

        // ======================================
        // PERFIL (opcional)
        // ======================================
        public async Task<IActionResult> Profile()
        {
            var id = HttpContext.Session.GetInt32("UsuarioId");
            if (id == null) return RedirectToAction("Login");

            var usuario = await _context.Usuario.Include(u => u.Persona)
                .FirstOrDefaultAsync(u => u.Id == id);

            return View(usuario);
        }
    }
}
