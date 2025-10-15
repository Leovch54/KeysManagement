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
            if (!EsAdmin())
                return RedirectToAction("Login", "Account");

            // 1️⃣ Obtener datos del docente
            var docente = (from u in _db.Usuario
                           join p in _db.Persona on u.Id equals p.Id
                           where u.Id == docenteId
                           select new
                           {
                               NombreCompleto = p.Nombres + " " + p.PrimerApellido + " " + (p.SegundoApellido ?? ""),
                               Email = u.Email
                           }).FirstOrDefault();

            if (docente == null)
            {
                ViewBag.Mensaje = "No se encontró al docente.";
                return View();
            }

            // 2️⃣ Obtener horarios (sin convertir TimeSpan en la base)
            var horariosRaw = _db.HorariosAcademico
                .Include(h => h.Materia)
                .Include(h => h.Aula)
                .Include(h => h.PeriodoAcademico)
                .Where(h => h.DocenteId == docenteId && h.Estado == true)
                .OrderBy(h => h.HoraInicio)
                .ToList(); // <-- Aquí forzamos la ejecución en memoria

            // 3️⃣ Conversión de TimeSpan a string legible (en memoria)
            var horarios = horariosRaw.Select(h => new
            {
                HoraInicio = h.HoraInicio.ToString(@"hh\:mm"),
                HoraFin = h.HoraFin.ToString(@"hh\:mm"),
                h.Grupo,
                h.Lunes,
                h.Martes,
                h.Miercoles,
                h.Jueves,
                h.Viernes,
                h.Sabado,
                Materia = h.Materia != null ? h.Materia.Nombre : "Sin materia",
                Aula = h.Aula != null ? h.Aula.Codigo : "Sin aula",
                Periodo = h.PeriodoAcademico != null ? h.PeriodoAcademico.Nombre : "Sin periodo"
            }).ToList();

            // 4️⃣ Enviar datos a la vista
            ViewBag.Docente = docente;
            ViewBag.Horarios = horarios;
            Console.WriteLine("HORARIOS ENCONTRADOS:");
            foreach (var h in horarios)
            {
                Console.WriteLine($"{h.Materia} | {h.HoraInicio}-{h.HoraFin} | L:{h.Lunes} M:{h.Martes} Mi:{h.Miercoles} J:{h.Jueves} V:{h.Viernes} S:{h.Sabado}");
            }


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

            // Mantener valores ingresados en caso de error
            ViewBag.Nombres = nombres;
            ViewBag.PrimerApellido = primerApellido;
            ViewBag.SegundoApellido = segundoApellido;
            ViewBag.Telefono = telefono;
            ViewBag.Tipo = tipo;
            ViewBag.Email = email;
            ViewBag.Rol = rol;

            // 🔹 Validación: Campos requeridos
            if (string.IsNullOrWhiteSpace(nombres) || string.IsNullOrWhiteSpace(primerApellido) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(rol))
            {
                ViewBag.Mensaje = "⚠ Por favor, complete todos los campos obligatorios.";
                return View();
            }

            // 🔹 Normalizar espacios y capitalizar nombres/apellidos
            nombres = NormalizarNombre(nombres);
            primerApellido = NormalizarNombre(primerApellido);
            segundoApellido = NormalizarNombre(segundoApellido);
            telefono = telefono?.Trim();
            email = email.Trim().ToLower();

            // 🔹 Validación: Email duplicado
            if (_db.Usuario.Any(u => u.Email.ToLower() == email))
            {
                ViewBag.Mensaje = "❌ El correo electrónico ya está registrado.";
                return View();
            }

            // 🔹 Validación: Teléfono duplicado (solo si hay número)
            if (!string.IsNullOrWhiteSpace(telefono) &&
                _db.Persona.Any(p => p.Telefono != null && p.Telefono.Trim() == telefono))
            {
                ViewBag.Mensaje = "❌ El número de teléfono ya está registrado.";
                return View();
            }

            // 🔹 Crear persona
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

            // 🔹 Generar contraseña aleatoria y encriptar
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
                message.From = new MailAddress("alberth22291@gmail.com", "Sistema Gestión Llaves");
                message.To.Add(email);
                message.Subject = "Cuenta creada - Sistema Gestión de Llaves";
                message.Body = body;
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("alberth22291@gmail.com", "zcnf zlha zflh ttcv");
                    smtp.EnableSsl = true;
                    smtp.Send(message);
                }

                ViewBag.Mensaje = "✅ Usuario creado correctamente. Se envió un correo con la contraseña temporal.";
                ViewBag.Limpiar = true;
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = "⚠ Usuario creado, pero no se pudo enviar el correo: " + ex.Message;
                ViewBag.Limpiar = true;
            }

            return View();
        }

        // 🔧 MÉTODO AUXILIAR PARA FORMATEAR NOMBRES Y APELLIDOS
        private string NormalizarNombre(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "";

            // Eliminar espacios al inicio y al final
            texto = texto.Trim();

            // Reemplazar múltiples espacios por uno solo
            while (texto.Contains("  "))
                texto = texto.Replace("  ", " ");

            // Capitalizar cada palabra
            var palabras = texto.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < palabras.Length; i++)
            {
                var palabra = palabras[i].ToLower();
                palabras[i] = char.ToUpper(palabra[0]) + palabra.Substring(1);
            }

            return string.Join(" ", palabras);
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
