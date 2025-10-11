using Microsoft.AspNetCore.Mvc;
using Sistema_Gestion_Llaves.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System;
using System.Net;
using System.Net.Mail;

namespace Sistema_Gestion_Llaves.Controllers
{
    public class AdminDashboardController : Controller
    {
        private readonly SistemaGestionLlavesContext _context;

        public AdminDashboardController(SistemaGestionLlavesContext context)
        {
            _context = context;
        }
        public IActionResult VerHorarios(int docenteId)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioEmail")) ||
                HttpContext.Session.GetString("UsuarioRol")?.ToLower() != "admin")
            {
                return RedirectToAction("Index", "Login");
            }

            var docente = (from u in _context.Usuarios
                           join p in _context.Personas on u.Id equals p.Id
                           where u.Id == docenteId
                           select new
                           {
                               NombreCompleto = p.Nombres + " " + p.PrimerApellido + " " + p.SegundoApellido,
                               Email = u.Email
                           }).FirstOrDefault();

            var horarios = _context.HorarioAcademicos
                .Where(h => h.DocenteId == docenteId)
                .Select(h => new
                {
                    Dia = h.Dia,           // Lunes, Martes, etc.
                    HoraInicio = h.HoraInicio,
                    HoraFin = h.HoraFin,
                    Aula = h.Aula,
                    Materia = h.Materia
                }).ToList();

            ViewBag.Docente = docente;
            ViewBag.Horarios = horarios;

            return View();
        }

        // GET: Ver Docentes
        public IActionResult VerDocentes()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioEmail")) ||
                HttpContext.Session.GetString("UsuarioRol")?.ToLower() != "admin")
            {
                return RedirectToAction("Index", "Login");
            }

            var docentes = (from u in _context.Usuarios
                            join p in _context.Personas on u.Id equals p.Id
                            where u.Activo && (u.Rol.ToLower() == "docente" || p.Tipo.ToLower() == "docente")
                            select new
                            {
                                UsuarioId = u.Id,
                                Email = u.Email,
                                Rol = u.Rol,
                                Nombres = p.Nombres,
                                PrimerApellido = p.PrimerApellido,
                                SegundoApellido = p.SegundoApellido,
                                Telefono = p.Telefono,
                                Tipo = p.Tipo
                            }).ToList();

            ViewBag.Docentes = docentes;
            return View();
        }


        // GET: Dashboard Admin
        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioEmail")) ||
                HttpContext.Session.GetString("UsuarioRol")?.ToLower() != "admin")
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Email = HttpContext.Session.GetString("UsuarioEmail");
            ViewBag.Rol = HttpContext.Session.GetString("UsuarioRol");

            return View();
        }

        // GET: Ver Usuarios
        public IActionResult VerUsuarios()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioEmail")) ||
                HttpContext.Session.GetString("UsuarioRol")?.ToLower() != "admin")
            {
                return RedirectToAction("Index", "Login");
            }

            // Traer usuarios activos y combinarlos con Persona para mostrar info
            var usuariosConPersona = _context.Usuarios
                .Where(u => u.Activo)
                .Select(u => new
                {
                    UsuarioId = u.Id,
                    Email = u.Email,
                    Rol = u.Rol,
                    PrimeraVez = u.PrimeraVez,
                    Persona = _context.Personas.FirstOrDefault(p => p.Id == u.Id) // Vinculación temporal por posición
                })
                .ToList();

            ViewBag.Usuarios = usuariosConPersona;
            return View();
        }

        // POST: Eliminar Usuario
        [HttpPost]
        public IActionResult EliminarUsuario(int usuarioId)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioEmail")) ||
                HttpContext.Session.GetString("UsuarioRol")?.ToLower() != "admin")
            {
                return RedirectToAction("Index", "Login");
            }

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
            if (usuario != null)
            {
                usuario.Activo = false; // Eliminación lógica
                _context.SaveChanges();
            }

            return RedirectToAction("VerUsuarios");
        }

        // GET: Registrar Usuario
        [HttpGet]
        public IActionResult RegistrarUsuario()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioEmail")) ||
                HttpContext.Session.GetString("UsuarioRol")?.ToLower() != "admin")
            {
                return RedirectToAction("Index", "Login");
            }

            return View();
        }

        // POST: Registrar Usuario (Persona + Usuario)
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
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioEmail")) ||
                HttpContext.Session.GetString("UsuarioRol")?.ToLower() != "admin")
            {
                return RedirectToAction("Index", "Login");
            }

            if (_context.Usuarios.Any(u => u.Email == email))
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
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            _context.Personas.Add(persona);
            _context.SaveChanges();

            string contraseñaGenerada = Guid.NewGuid().ToString().Substring(0, 8);

            var usuario = new Usuario
            {
                Id = persona.Id,
                Email = email,
                Pin = contraseñaGenerada,
                Rol = rol,
                Activo = true,
                FechaCreacion = DateTime.Now,
                PrimeraVez = true
            };

            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            // Enviar correo
            try
            {
                var body = $@"
<h2>Bienvenido al Sistema Gestión de Llaves</h2>
<p>Hola {nombres} {primerApellido},</p>
<p>Se ha creado tu usuario con los siguientes datos:</p>
<ul>
    <li>Email: {email}</li>
    <li>Contraseña: {contraseñaGenerada}</li>
    <li>Rol: {rol}</li>
</ul>
<p>Te recomendamos cambiar tu contraseña después de iniciar sesión.</p>";

                var message = new MailMessage();
                message.From = new MailAddress("alberth22291@gmail.com", "Sistema Gestión Llaves");
                message.To.Add(email);
                message.Subject = "Registro de Usuario - Sistema Gestión de Llaves";
                message.Body = body;
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("alberth22291@gmail.com", "zcnf zlha zflh ttcv");
                    smtp.EnableSsl = true;
                    smtp.Send(message);
                }

                ViewBag.Mensaje = "Usuario registrado correctamente y correo enviado.";
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = "Usuario registrado, pero no se pudo enviar el correo: " + ex.Message;
            }

            return View();
        }
    }
}
