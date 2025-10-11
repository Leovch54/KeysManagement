using GestionLlaves.Data;
using GestionLlaves.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace GestionLlaves.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        public AdminController(AppDbContext db)
        {
            _db = db;
        }

        // ============================
        // PANTALLA PRINCIPAL DEL ADMIN
        // ============================
        public IActionResult Index()
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Account");

            ViewBag.Email = HttpContext.Session.GetString("UsuarioEmail");
            return View();
        }

        // ============================
        // VER DOCENTES
        // ============================
        public IActionResult VerDocentes()
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Account");

            var docentes = (from u in _db.Usuario
                            join p in _db.Persona on u.Id equals p.Id
                            where u.Estado == true && u.Rol.ToLower() == "docente"
                            select new
                            {
                                Id = u.Id,
                                Email = u.Email,
                                Nombres = p.Nombres,
                                PrimerApellido = p.PrimerApellido,
                                SegundoApellido = p.SegundoApellido,
                                Telefono = p.Telefono,
                                Tipo = p.Tipo
                            }).ToList();

            ViewBag.Docentes = docentes;
            return View();
        }

        // ============================
        // VER HORARIOS DE UN DOCENTE
        // ============================
        public IActionResult VerHorarios(int docenteId)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Account");

            var docente = (from u in _db.Usuario
                           join p in _db.Persona on u.Id equals p.Id
                           where u.Id == docenteId
                           select new
                           {
                               NombreCompleto = p.Nombres + " " + p.PrimerApellido,
                               Email = u.Email
                           }).FirstOrDefault();

            var horarios = _db.HorariosAcademico
                .Include(h => h.Materia)
                .Include(h => h.Aula)
                .Where(h => h.DocenteId == docenteId)
                .Select(h => new
                {
                    Lunes = h.Lunes,
                    Martes = h.Martes,
                    Miercoles = h.Miercoles,
                    Jueves = h.Jueves,
                    Viernes = h.Viernes,
                    Sabado = h.Sabado,
                    HoraInicio = h.HoraInicio,
                    HoraFin = h.HoraFin,
                    Aula = h.Aula!.Codigo,
                    Materia = h.Materia!.Nombre
                }).ToList();

            ViewBag.Docente = docente;
            ViewBag.Horarios = horarios;
            return View();
        }

        // ============================
        // VER TODOS LOS USUARIOS
        // ============================
        public IActionResult VerUsuarios()
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Account");

            var usuarios = (from u in _db.Usuario
                            join p in _db.Persona on u.Id equals p.Id
                            where u.Estado == true
                            select new
                            {
                                Id = u.Id,
                                Email = u.Email,
                                Rol = u.Rol,
                                Nombres = p.Nombres,
                                PrimerApellido = p.PrimerApellido,
                                Tipo = p.Tipo
                            }).ToList();

            ViewBag.Usuarios = usuarios;
            return View();
        }

        // ============================
        // REGISTRAR NUEVO USUARIO (GET)
        // ============================
        [HttpGet]
        public IActionResult RegistrarUsuario()
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Account");
            return View();
        }

        // ============================
        // REGISTRAR NUEVO USUARIO (POST)
        // ============================
        [HttpPost]
        public IActionResult RegistrarUsuario(
            string nombres,
            string primerApellido,
            string segundoApellido,
            string telefono,
            string tipo,
            string email,
            string rol)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Account");

            if (_db.Usuario.Any(u => u.Email == email))
            {
                ViewBag.Mensaje = "El correo ya está registrado.";
                return View();
            }

            var persona = new Persona
            {
                Nombres = nombres,
                PrimerApellido = primerApellido,
                SegundoApellido = segundoApellido,
                Telefono = telefono,
                Tipo = tipo,
                Estado = true,
                FechaCreacion = DateTime.Now,
                CreadoModPor = 1
            };

            _db.Persona.Add(persona);
            _db.SaveChanges();

            string contrasena = Guid.NewGuid().ToString().Substring(0, 8);
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(contrasena));

            var usuario = new Usuario
            {
                Id = persona.Id,
                Email = email,
                Contrasenia = hash,
                Rol = rol.ToUpper(),
                Estado = true,
                FechaCreacion = DateTime.Now,
                CreadoModPor = 1
            };

            _db.Usuario.Add(usuario);
            _db.SaveChanges();

            // ---------- Enviar correo ----------
            try
            {
                var body = $@"
                <h2>Bienvenido al Sistema de Gestión de Llaves</h2>
                <p>Hola {nombres} {primerApellido},</p>
                <p>Tu cuenta ha sido creada con éxito.</p>
                <ul>
                    <li><strong>Email:</strong> {email}</li>
                    <li><strong>Contraseña temporal:</strong> {contrasena}</li>
                    <li><strong>Rol:</strong> {rol}</li>
                </ul>
                <p>Te recomendamos cambiar la contraseña después de iniciar sesión.</p>";

                var message = new MailMessage();
                message.From = new MailAddress("tucorreo@gmail.com", "Sistema Gestión Llaves");
                message.To.Add(email);
                message.Subject = "Cuenta creada - Sistema Gestión de Llaves";
                message.Body = body;
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("tucorreo@gmail.com", "tu-contraseña-o-clave-app");
                    smtp.EnableSsl = true;
                    smtp.Send(message);
                }

                ViewBag.Mensaje = "Usuario creado correctamente. Correo enviado con la contraseña temporal.";
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = "Usuario creado, pero no se pudo enviar el correo: " + ex.Message;
            }

            return View();
        }

        // ============================
        // ELIMINAR USUARIO (LÓGICO)
        // ============================
        [HttpPost]
        public IActionResult EliminarUsuario(int usuarioId)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Account");

            var usuario = _db.Usuario.FirstOrDefault(u => u.Id == usuarioId);
            if (usuario != null)
            {
                usuario.Estado = false; // eliminación lógica
                _db.SaveChanges();
            }

            return RedirectToAction("VerUsuarios");
        }

        // ============================
        // MÉTODO AUXILIAR
        // ============================
        private bool EsAdmin()
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            var email = HttpContext.Session.GetString("UsuarioEmail");
            return !string.IsNullOrEmpty(email) && rol?.ToLower() == "admin";
        }
    }
}
