using GestionLlaves.Data;
using GestionLlaves.Helpers;
using GestionLlaves.Models;
using iTextSharp.text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using GestionLlaves.Models.ViewModels;


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
        // ============================
        // DASHBOARD PRINCIPAL DEL ADMIN
        // ============================
        public IActionResult Index()
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Account");
            ViewBag.LlavesPendientes = ContarLlavesPendientes();
            ViewBag.Email = HttpContext.Session.GetString("UsuarioEmail");

            return View();
        }

        // ============================
        // API PARA DASHBOARD EN TIEMPO REAL
        // ============================
        [HttpGet]
        public JsonResult ObtenerEstadisticasDashboard()
        {
            try
            {
                // 1️⃣ Llaves en uso (préstamos aprobados sin devolver)
                var llavesEnUso = _db.Prestamo
                    .Count(p => p.EstadoPrestamo == "APROBADO" && p.FechaFinReal == null && p.Estado);

                // 2️⃣ Aulas disponibles
                var totalAulas = _db.Aula.Count(a => a.Estado);
                var aulasOcupadas = _db.Prestamo
                    .Where(p => p.EstadoPrestamo == "APROBADO" && p.FechaFinReal == null && p.Estado )
                    .Select(p => p.AulaId)
                    .Distinct()
                    .Count();
                var aulasDisponibles = totalAulas - aulasOcupadas;

                // 3️⃣ Préstamos hoy
                var hoy = DateTime.Today;
                var manana = hoy.AddDays(1);
                var prestamosHoy = _db.Prestamo
                    .Count(p => p.FechaInicio >= hoy && p.FechaInicio < manana && p.Estado);

                // 4️⃣ Top 5 docentes con mayor retraso
                var prestamosConRetraso = _db.Prestamo
                    .Include(p => p.Persona)
                        .ThenInclude(per => per.Usuario)
                    .Where(p => p.FechaFinReal != null &&
                               p.FechaFinReal > p.FechaFinProgramada &&
                               p.Estado)
                    .ToList();

                var docentesMorosos = prestamosConRetraso
                    .GroupBy(p => new
                    {
                        p.PersonaId,
                        NombreCompleto = p.Persona.Nombres + " " +
                                        p.Persona.PrimerApellido +
                                        (string.IsNullOrEmpty(p.Persona.SegundoApellido) ? "" : " " + p.Persona.SegundoApellido),
                        Email = p.Persona.Usuario != null ? p.Persona.Usuario.Email : "Sin email"
                    })
                    .Select(g => new
                    {
                        NombreCompleto = g.Key.NombreCompleto,
                        Email = g.Key.Email,
                        TotalRetrasos = g.Count(),
                        PromedioRetrasoHoras = g.Average(p =>
                            (p.FechaFinReal.Value - p.FechaFinProgramada).TotalHours)
                    })
                    .OrderByDescending(x => x.PromedioRetrasoHoras)
                    .Take(5)
                    .ToList();

                return Json(new
                {
                    llavesEnUso,
                    aulasDisponibles,
                    prestamosHoy,
                    docentesMorosos
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Dashboard: {ex.Message}");
                return Json(new
                {
                    llavesEnUso = 0,
                    aulasDisponibles = 0,
                    prestamosHoy = 0,
                    docentesMorosos = new List<object>()
                });
            }
        }


        // ============================
        // VER DOCENTES
        // ============================
        public IActionResult VerDocentes()
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Account");
            ViewBag.LlavesPendientes = ContarLlavesPendientes();




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
            ViewBag.LlavesPendientes = ContarLlavesPendientes();


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
            var horariosRaw = _db.HorarioAcademico
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
            ViewBag.LlavesPendientes = ContarLlavesPendientes();


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
            ViewBag.LlavesPendientes = ContarLlavesPendientes();


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
            string email,
            string rol)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Account");
            ViewBag.LlavesPendientes = ContarLlavesPendientes();

            const string tipoUsuario = "DOCENTE";

            // Mantener valores ingresados en caso de error
            ViewBag.Nombres = nombres;
            ViewBag.PrimerApellido = primerApellido;
            ViewBag.SegundoApellido = segundoApellido;
            ViewBag.Telefono = telefono;
            ViewBag.Email = email;
            ViewBag.Rol = rol;

            // 🔹 Validación: Campos requeridos (excepto segundoApellido)
            if (string.IsNullOrWhiteSpace(nombres) || string.IsNullOrWhiteSpace(primerApellido) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(rol) ||
                string.IsNullOrWhiteSpace(telefono))
            {
                ViewBag.Mensaje = "⚠ Por favor, complete todos los campos obligatorios marcados con (*).";
                return View();
            }

            // 🔹 Normalización
            nombres = NormalizarNombre(nombres);
            primerApellido = NormalizarNombre(primerApellido);
            segundoApellido = NormalizarNombre(segundoApellido);
            telefono = telefono.Trim();
            email = email.Trim().ToLower();

            const string regexSoloLetrasEspacios = @"^[a-zA-Z\sáéíóúÁÉÍÓÚñÑ]+$";

            // 🔹 Validación: Formato (Solo letras y espacios)
            if (!Regex.IsMatch(nombres, regexSoloLetrasEspacios) ||
                !Regex.IsMatch(primerApellido, regexSoloLetrasEspacios) ||
                (!string.IsNullOrWhiteSpace(segundoApellido) && !Regex.IsMatch(segundoApellido, regexSoloLetrasEspacios)))
            {
                ViewBag.Mensaje = "❌ Los nombres y apellidos solo pueden contener letras y espacios.";
                return View();
            }

            // 🔹 Validación: Formato de Teléfono (solo números)
            if (!Regex.IsMatch(telefono, @"^[0-9]+$"))
            {
                ViewBag.Mensaje = "❌ El campo Teléfono solo puede contener números.";
                return View();
            }

            // 🔹 Validación: Email duplicado
            if (_db.Usuario.Any(u => u.Email.ToLower() == email))
            {
                ViewBag.Mensaje = "❌ El correo electrónico ya está registrado.";
                return View();
            }

            // 🔹 Validación: Teléfono duplicado
            if (_db.Persona.Any(p => p.Telefono != null && p.Telefono.Trim() == telefono))
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
                Tipo = tipoUsuario,
                Estado = true,
                FechaCreacion = DateTime.Now,
                CreadoModPor = 1
            };
            _db.Persona.Add(persona);
            _db.SaveChanges();

            // 🔹 Generar contraseña aleatoria y encriptar
            string contrasena = Guid.NewGuid().ToString().Substring(0, 8);
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(contrasena));

            // 🔹 Crear usuario
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

            // 🔹 Ensamblar nombre completo para el saludo
            string nombreCompleto = $"{nombres} {primerApellido}";
            if (!string.IsNullOrWhiteSpace(segundoApellido))
            {
                nombreCompleto += $" {segundoApellido}";
            }

            // 🔹 Diseño de correo mejorado
            var body = $@"
<div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.05);'>
    
    <div style='background-color: #72103A; color: white; padding: 25px 30px; text-align: center; border-bottom: 5px solid #5d0c2e;'>
        <h2 style='margin: 0; font-size: 26px; font-weight: bold;'>Sistema de Gestión de Llaves</h2>
    </div>
    
    <div style='padding: 30px;'>
        <p style='font-size: 16px;'>Estimado(a) <strong>{nombreCompleto}</strong>,</p>
        
        <p>Tu cuenta ha sido creada con éxito. Puedes acceder al sistema con las siguientes credenciales temporales:</p>
        
        <div style='background-color: #f5f5f5; padding: 20px; border-radius: 8px; border: 1px solid #eee; margin: 30px 0;'>
            <table style='width: 100%; border-collapse: collapse;'>
                <tr>
                    <td style='padding: 10px 0; font-weight: bold; width: 40%;'>Email:</td>
                    <td style='padding: 10px 0;'>{email}</td>
                </tr>
                <tr>
                    <td style='padding: 10px 0; font-weight: bold;'>Contraseña Temporal:</td>
                    <td style='padding: 10px 0; color: #72103A; font-size: 18px; font-weight: bold;'>{contrasena}</td>
                </tr>
                <tr>
                    <td style='padding: 10px 0; font-weight: bold;'>Rol Asignado:</td>
                    <td style='padding: 10px 0;'>{rol}</td>
                </tr>
            </table>
        </div>

        <p style='margin-top: 30px; padding: 15px; background-color: #fff3cd; border-left: 5px solid #ffc107; font-size: 14px; border-radius: 4px;'>
            <strong>IMPORTANTE:</strong> Por motivos de seguridad, deberás **cambiar esta contraseña temporal** obligatoriamente en tu primer inicio de sesión.
        </p>
        
        <p style='text-align: center; margin-top: 30px;'>
            <a href='https://tudominio.com/Account/Login' style='display: inline-block; background-color: #72103A; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold; font-size: 16px;'>
                Ir al Sistema
            </a>
        </p>
    </div>
    
    <div style='background-color: #f8f8f8; color: #999; padding: 15px 30px; text-align: center; font-size: 12px; border-top: 1px solid #eee;'>
        Este es un mensaje automático. Por favor, no responder a esta dirección de correo.
    </div>
</div>";


            try
            {
                var message = new MailMessage();
                message.From = new MailAddress("alberth22291@gmail.com", "Sistema Gestión Llaves");
                message.To.Add(email);
                message.Subject = "Cuenta Creada - Credenciales de Acceso";
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

            texto = texto.Trim();
            while (texto.Contains("  "))
                texto = texto.Replace("  ", " ");

            var palabras = texto.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < palabras.Length; i++)
            {
                var palabra = palabras[i].ToLower();
                if (palabra.Length > 0)
                {
                    palabras[i] = char.ToUpper(palabra[0]) + palabra.Substring(1);
                }
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
            ViewBag.LlavesPendientes = ContarLlavesPendientes();


            var usuario = _db.Usuario.FirstOrDefault(u => u.Id == usuarioId);
            if (usuario != null)
            {
                usuario.Estado = false; // eliminación lógica
                _db.SaveChanges();
            }

            return RedirectToAction("VerUsuarios");
        }


        // ============================
        // MÓDULO EXTRAS (LIMPIEZA / MANTENIMIENTO / ESTUDIANTE)
        // ============================
        [HttpGet]
        public IActionResult Extras()
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Account");
            ViewBag.LlavesPendientes = ContarLlavesPendientes();


            var extras = _db.Reserva
                .Include(r => r.Aula)
                .Where(r => r.Proposito == "Limpieza" || r.Proposito == "Mantenimiento" || r.Proposito == "Estudiante")
                .OrderByDescending(r => r.FechaCreacion)
                .ToList();

            ViewBag.Aulas = _db.Aula
                .Include(a => a.Edificio)
                .Where(a => a.Estado == true)
                .ToList();

            return View(extras);
        }

        [HttpPost]
        public IActionResult AgregarExtra(string area, string nombre, int aulaId)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Account");
            ViewBag.LlavesPendientes = ContarLlavesPendientes();


            var usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 1;

            var extra = new Reserva
            {
                SolicitanteId = usuarioId,
                AulaId = aulaId,
                FechaInicio = DateTime.Now,
                FechaFin = DateTime.Now.AddMinutes(5),
                Proposito = area,         // Limpieza / Mantenimiento / Estudiante
                Justificacion = nombre,   // Nombre de la persona o detalle
                EstadoReserva = "Pendiente",
                Estado = true,
                FechaCreacion = DateTime.Now,
                CreadoModPor = usuarioId
            };

            _db.Reserva.Add(extra);
            _db.SaveChanges();

            // 🔹 Guardar en archivo JSON
            var reportes = ReporteHelper.CargarReportes();

            var aula = _db.Aula.Include(a => a.Edificio).FirstOrDefault(a => a.Id == aulaId);
            var codigoAula = aula != null ? aula.Codigo : "N/A";

            var nuevoReporte = new Reportes
            {
                Id = extra.Id,
                Area = area,
                Nombre = nombre,
                Aula = codigoAula,
                FechaSolicitud = DateTime.Now,
                Estado = "Pendiente"
            };

            reportes.Add(nuevoReporte);

            // Ordenar por fecha descendente
            reportes = reportes.OrderByDescending(r => r.FechaSolicitud).ToList();

            ReporteHelper.GuardarReportes(reportes);

            return RedirectToAction("Extras");
        }

        [HttpPost]
        public IActionResult CambiarEstadoExtra(int id)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Account");
            ViewBag.LlavesPendientes = ContarLlavesPendientes();

            var item = _db.Reserva.FirstOrDefault(r => r.Id == id);
            if (item != null)
            {
                item.EstadoReserva = item.EstadoReserva == "Pendiente" ? "Devuelto" : "Pendiente";
                _db.SaveChanges();

                // 🔹 Actualizar en archivo JSON
                var reportes = ReporteHelper.CargarReportes();
                var reporte = reportes.FirstOrDefault(r => r.Id == id);

                if (reporte != null)
                {
                    if (item.EstadoReserva == "Devuelto")
                    {
                        reporte.Estado = "Devuelto";
                        reporte.FechaDevolucion = DateTime.Now;
                    }
                    else
                    {
                        reporte.Estado = "Pendiente";
                        reporte.FechaDevolucion = null;
                    }

                    ReporteHelper.GuardarReportes(reportes);
                }
            }

            return RedirectToAction("Extras");
        }

        // ✅ Método auxiliar para verificar rol admin
        private bool EsAdmin()
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            var email = HttpContext.Session.GetString("UsuarioEmail");
            return !string.IsNullOrEmpty(email) && rol?.ToLower() == "admin";
        }
        [HttpGet]
        public IActionResult Reportes()
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Account");
            ViewBag.LlavesPendientes = ContarLlavesPendientes();

            var reportes = ReporteHelper.CargarReportes();
            ViewBag.Reportes = reportes.OrderByDescending(r => r.FechaSolicitud).ToList();

            return View();
        }
        // ============================
        // MÓDULO DE SOLICITUDES (DOCENTES → ADMIN)
        // ============================
        [HttpGet]
        // 📂 Archivo: Controllers/AdminController.cs

        // ============================
        // MÓDULO DE SOLICITUDES (DOCENTES → ADMIN)
        // ============================
        [HttpGet]
        public IActionResult Solicitudes()
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Account");
            ViewBag.LlavesPendientes = ContarLlavesPendientes();

            var listaSolicitudes = new List<AdminSolicitudViewModel>();

            // 1️⃣ Obtener PRÉSTAMOS pendientes ("ACTIVO")
            // (Aunque ahora se auto-aprueban, dejamos esto por si acaso alguna queda en este estado)
            var prestamosPendientes = _db.Prestamo
                .Include(p => p.Persona)
                .Include(p => p.Aula).ThenInclude(a => a.Edificio)
                .Where(p => p.EstadoPrestamo == "ACTIVO" && p.Estado)
                .Select(p => new AdminSolicitudViewModel
                {
                    Id = p.Id,
                    TipoSolicitud = "PRESTAMO",
                    DocenteNombre = $"{p.Persona.Nombres} {p.Persona.PrimerApellido}",
                    AulaCodigo = p.Aula.Codigo,
                    EdificioNombre = p.Aula.Edificio.Nombre,
                    FechaInicio = p.FechaInicio,
                    FechaFin = p.FechaFinProgramada,
                    Estado = p.EstadoPrestamo,
                    Detalle = p.Tipo, // Ejemplo: "REGULAR", "EXCEPCIONAL"
                    FechaSolicitud = p.FechaCreacion
                }).ToList();
            listaSolicitudes.AddRange(prestamosPendientes);

            // 2️⃣ Obtener RESERVAS pendientes ("PENDIENTE") - ¡ESTO ES LO NUEVO!
            var reservasPendientes = _db.Reserva
                .Include(r => r.Solicitante).ThenInclude(u => u.Persona)
                .Include(r => r.Aula).ThenInclude(a => a.Edificio)
                .Where(r => r.EstadoReserva == "PENDIENTE" && r.Estado)
                .Select(r => new AdminSolicitudViewModel
                {
                    Id = r.Id,
                    TipoSolicitud = "RESERVA",
                    DocenteNombre = $"{r.Solicitante.Persona.Nombres} {r.Solicitante.Persona.PrimerApellido}",
                    AulaCodigo = r.Aula.Codigo,
                    EdificioNombre = r.Aula.Edificio.Nombre,
                    FechaInicio = r.FechaInicio,
                    FechaFin = r.FechaFin,
                    Estado = r.EstadoReserva,
                    Detalle = r.Proposito, // El propósito de la reserva
                    FechaSolicitud = r.FechaCreacion
                }).ToList();
            listaSolicitudes.AddRange(reservasPendientes);

            // Ordenar todas por fecha de solicitud descendente
            listaSolicitudes = listaSolicitudes.OrderByDescending(s => s.FechaSolicitud).ToList();

            // Enviamos la lista combinada a la vista
            return View(listaSolicitudes);
        }
        [HttpPost]
        // 📂 Archivo: Controllers/AdminController.cs

        [HttpPost]
        // Se añade el parámetro 'tipoSolicitud' para saber en qué tabla buscar
        public IActionResult CambiarEstadoSolicitud(int id, string estado, string tipoSolicitud)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Account");
            ViewBag.LlavesPendientes = ContarLlavesPendientes();

            if (tipoSolicitud == "PRESTAMO")
            {
                var prestamo = _db.Prestamo.FirstOrDefault(p => p.Id == id);
                if (prestamo != null)
                {
                    prestamo.EstadoPrestamo = estado.ToUpper(); // APROBADO o RECHAZADO
                    prestamo.UltimaMod = DateTime.Now;
                    _db.SaveChanges();
                }
            }
            // 📂 Archivo: Controllers/AdminController.cs
            // Método: CambiarEstadoSolicitud

            // ... (el inicio del método y el bloque if (tipoSolicitud == "PRESTAMO") siguen igual) ...

            else if (tipoSolicitud == "RESERVA")
            {
                // Necesitamos incluir datos para poder crear el préstamo después
                var reserva = _db.Reserva.FirstOrDefault(r => r.Id == id);

                if (reserva != null)
                {
                    // Verificar si se está APROBANDO o RECHAZANDO
                    if (estado.ToUpper() == "APROBADO")
                    {
                        // 1. Actualizar el estado de la reserva
                        reserva.EstadoReserva = "APROBADA";
                        reserva.AprobadaPor = HttpContext.Session.GetInt32("UsuarioId");
                        reserva.FechaAprobacion = DateTime.Now;
                        reserva.UltimaMod = DateTime.Now;

                        // 2. ▼▼▼ NUEVO: CREAR AUTOMÁTICAMENTE EL PRÉSTAMO ▼▼▼
                        // Al aprobar la reserva, asumimos que se entrega la llave en ese momento.
                        // Verificamos que no exista ya un préstamo para esta reserva para evitar duplicados.
                        bool existePrestamo = _db.Prestamo.Any(p => p.ReservaId == reserva.Id);

                        if (!existePrestamo)
                        {
                            var adminId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

                            var nuevoPrestamo = new Prestamo
                            {
                                PersonaId = reserva.SolicitanteId, // El docente que solicitó la reserva
                                AulaId = reserva.AulaId,
                                ReservaId = reserva.Id, // Vinculamos a la reserva
                                HorarioAcademicoId = null,

                                // La llave se entrega AHORA MISMO
                                FechaInicio = DateTime.Now,
                                // La fecha de devolución es la que estaba planeada en la reserva
                                FechaFinProgramada = reserva.FechaFin,

                                Tipo = "RESERVA",
                                EstadoPrestamo = "APROBADO", // ¡Directo a estado APROBADO!
                                Estado = true,
                                FechaCreacion = DateTime.Now,
                                CreadoModPor = adminId,
                                RecibidoPor = adminId // El admin actual entregó la llave
                            };

                            _db.Prestamo.Add(nuevoPrestamo);
                        }
                        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
                    }
                    else
                    {
                        // Si se rechaza, solo actualizamos la reserva
                        reserva.EstadoReserva = "RECHAZADA";
                        reserva.AprobadaPor = HttpContext.Session.GetInt32("UsuarioId");
                        reserva.FechaAprobacion = DateTime.Now;
                        reserva.UltimaMod = DateTime.Now;
                    }

                    // Guardamos todos los cambios (reserva y posible nuevo préstamo)
                    _db.SaveChanges();
                }
            }

            return RedirectToAction("Solicitudes");
        }
        [HttpGet]
        [HttpGet]
        public IActionResult Llaves()
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Account");
            ViewBag.LlavesPendientes = ContarLlavesPendientes();

            var llavesActivas = _db.Prestamo
                .Include(p => p.Persona)
                    .ThenInclude(per => per.Usuario)
                .Include(p => p.Aula)
                    .ThenInclude(a => a.Edificio)
                // .Include(p => p.Reserva) // Ya no es necesario aquí porque filtraremos las que tengan reserva
                // ▼▼▼▼▼▼▼▼ CAMBIO AQUÍ ▼▼▼▼▼▼▼▼
                // Agregamos "&& p.ReservaId == null" para mostrar SOLO clases normales
                .Where(p => p.EstadoPrestamo == "APROBADO" && p.FechaFinReal == null && p.Estado && p.ReservaId == null)
                // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
                .OrderBy(p => p.FechaInicio)
                .ToList();

            return View(llavesActivas);
        }
        // 📂 Archivo: Controllers/AdminController.cs

        // ============================
        // NUEVA PANTALLA: RESERVAS EN USO
        // ============================
        [HttpGet]
        public IActionResult Reservas()
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Account");
            ViewBag.LlavesPendientes = ContarLlavesPendientes();

            var reservasActivas = _db.Prestamo
                .Include(p => p.Persona).ThenInclude(per => per.Usuario)
                .Include(p => p.Aula).ThenInclude(a => a.Edificio)
                .Include(p => p.Reserva) // ¡Importante incluir la reserva aquí!
                                         // ▼▼▼▼▼▼▼▼ FILTRO ▼▼▼▼▼▼▼▼
                                         // Buscamos APROBADO, SIN DEVOLVER, y que SÍ tenga una Reserva ID
                .Where(p => p.EstadoPrestamo == "APROBADO" && p.FechaFinReal == null && p.Estado && p.ReservaId != null)
                // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
                .OrderBy(p => p.FechaInicio)
                .ToList();

            // Reutilizamos el mismo modelo (Prestamo), pero lo enviamos a una vista diferente
            return View("Reservas", reservasActivas);
        }




        [HttpPost]
        public IActionResult MarcarDevuelto(int id)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Account");
            ViewBag.LlavesPendientes = ContarLlavesPendientes();



            var prestamo = _db.Prestamo.FirstOrDefault(p => p.Id == id);
            if (prestamo != null)
            {
                prestamo.FechaFinReal = DateTime.Now;

                // Si el docente devolvió tarde o a tiempo
                if (prestamo.FechaFinReal > prestamo.FechaFinProgramada)
                    prestamo.EstadoPrestamo = "TARDE";
                else
                    prestamo.EstadoPrestamo = "A TIEMPO";

                _db.SaveChanges();
            }

            return RedirectToAction("Llaves");
        }
        private int ContarLlavesPendientes()
        {
            return _db.Prestamo
                .Count(p => p.EstadoPrestamo == "APROBADO" && p.FechaFinReal == null && p.Estado);
        }
        [HttpGet]
        public IActionResult ReporteLlaves(DateTime? desde, DateTime? hasta, string docente, string aula)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Account");

            ViewBag.LlavesPendientes = ContarLlavesPendientes();

            // 🔹 Si no hay fechas, usamos los últimos 7 días por defecto
            var fechaInicio = desde ?? DateTime.Today.AddDays(-7);
            var fechaFin = hasta ?? DateTime.Today.AddDays(1);

            ViewBag.Desde = fechaInicio.ToString("yyyy-MM-dd");
            ViewBag.Hasta = fechaFin.ToString("yyyy-MM-dd");
            ViewBag.Docente = docente; // Valor para mantener el filtro en la vista
            ViewBag.Aula = aula;       // 💡 Nuevo valor para mantener el filtro en la vista

            // 🔹 Consulta base filtrada por rango de fechas
            var query = _db.Prestamo
                .Include(p => p.Persona)
                .Include(p => p.Aula).ThenInclude(a => a.Edificio)
                .Where(p => p.FechaInicio >= fechaInicio && p.FechaInicio <= fechaFin);

            // 💡 Filtro por nombre de docente si se proporciona un valor
            if (!string.IsNullOrEmpty(docente))
            {
                var docenteFiltro = docente.Trim().ToLower();

                query = query.Where(p =>
                    (p.Persona.Nombres + " " + p.Persona.PrimerApellido).ToLower().Contains(docenteFiltro)
                );
            }

            // 💡 NUEVO FILTRO: Por código de Aula si se proporciona un valor
            if (!string.IsNullOrEmpty(aula))
            {
                var aulaFiltro = aula.Trim().ToLower();

                query = query.Where(p =>
                    p.Aula != null && p.Aula.Codigo.ToLower().Contains(aulaFiltro)
                );
            }

            // 🔹 Aplicamos orden y seleccionamos los datos finales
            var reportes = query
                .OrderByDescending(p => p.FechaInicio)
                .Select(p => new
                {
                    Docente = p.Persona != null ?
                        (p.Persona.Nombres + " " + p.Persona.PrimerApellido) : "—",
                    Aula = p.Aula != null ? p.Aula.Codigo : "—",
                    Edificio = p.Aula != null ? p.Aula.Edificio.Nombre : "—",
                    FechaInicio = p.FechaInicio,
                    FechaFinProgramada = p.FechaFinProgramada,
                    FechaFinReal = p.FechaFinReal,
                    Estado = p.EstadoPrestamo,
                    Retraso = (p.FechaFinReal != null && p.FechaFinReal > p.FechaFinProgramada)
                })
                .ToList();

            return View(reportes);
        }
        // --- LLAVES / REPORTES ---
        [HttpGet]


        // 🔽 AQUÍ PEGAS ESTE BLOQUE 🔽
        [HttpGet]
        public IActionResult SinDevolver()
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Account");

            ViewBag.LlavesPendientes = ContarLlavesPendientes();

            var ahora = DateTime.Now;

            var prestamosActivos = _db.Prestamo
                .Include(p => p.Persona)
                .Include(p => p.Aula).ThenInclude(a => a.Edificio)
                // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ CORRECCIÓN APLICADA AQUÍ ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
                // ANTES: .Where(p => p.EstadoPrestamo == "ACTIVO" && p.FechaFinReal == null)
                // AHORA: Buscamos ACTIVO O APROBADO que no se hayan devuelto
                .Where(p => (p.EstadoPrestamo == "ACTIVO" || p.EstadoPrestamo == "APROBADO") && p.FechaFinReal == null)
                // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
                .OrderByDescending(p => p.FechaInicio)
                .Select(p => new
                {
                    Docente = p.Persona != null ? $"{p.Persona.Nombres} {p.Persona.PrimerApellido}" : "—",
                    Aula = p.Aula != null ? p.Aula.Codigo : "—",
                    Edificio = p.Aula != null ? p.Aula.Edificio.Nombre : "—",
                    FechaInicio = p.FechaInicio,
                    FechaFinProgramada = p.FechaFinProgramada,
                    HorasRetraso = (ahora > p.FechaFinProgramada) ? (ahora - p.FechaFinProgramada).TotalHours : 0,
                    EnRetraso = ahora > p.FechaFinProgramada
                })
                .ToList();

            return View(prestamosActivos);
        }
        [HttpGet]
        public IActionResult DescargarReporteLlaves(DateTime? desde, DateTime? hasta)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Account");

            var fechaInicio = desde ?? DateTime.Today.AddDays(-7);
            var fechaFin = hasta ?? DateTime.Today.AddDays(1);

            var reportes = _db.Prestamo
                .Include(p => p.Persona)
                .Include(p => p.Aula).ThenInclude(a => a.Edificio)
                .Where(p => p.FechaInicio >= fechaInicio && p.FechaInicio <= fechaFin)
                .OrderByDescending(p => p.FechaInicio)
                .ToList();

            using (var stream = new MemoryStream())
            {
                var doc = new iTextSharp.text.Document();
                var writer = iTextSharp.text.pdf.PdfWriter.GetInstance(doc, stream);
                doc.Open();

                var fontTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var fontCabecera = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 9);

                // 🔹 Título
                doc.Add(new iTextSharp.text.Paragraph("Reporte de Llaves", fontTitulo));
                doc.Add(new iTextSharp.text.Paragraph($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}\n\n", fontNormal));

                // 🔹 Tabla
                var table = new iTextSharp.text.pdf.PdfPTable(8);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 2f, 1.2f, 1.3f, 1.5f, 1.5f, 1.5f, 1f, 0.8f });

                // Cabecera
                string[] headers = { "Docente", "Aula", "Edificio", "Inicio", "Fin Programado", "Devolución", "Estado", "Retraso" };
                foreach (var h in headers)
                {
                    var cell = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(h, fontCabecera));
                    cell.BackgroundColor = new iTextSharp.text.BaseColor(200, 0, 80);
                    cell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                    table.AddCell(cell);
                }

                // Filas
                foreach (var p in reportes)
                {
                    string docente = p.Persona != null ? $"{p.Persona.Nombres} {p.Persona.PrimerApellido}" : "—";
                    string aula = p.Aula?.Codigo ?? "—";
                    string edificio = p.Aula?.Edificio?.Nombre ?? "—";
                    string inicio = p.FechaInicio.ToString("dd/MM HH:mm");
                    string fin = p.FechaFinProgramada.ToString("dd/MM HH:mm");
                    string devolucion = p.FechaFinReal?.ToString("dd/MM HH:mm") ?? "—";
                    string estado = p.EstadoPrestamo;
                    string retraso = (p.FechaFinReal != null && p.FechaFinReal > p.FechaFinProgramada) ? "Sí" : "No";

                    table.AddCell(new iTextSharp.text.Phrase(docente, fontNormal));
                    table.AddCell(new iTextSharp.text.Phrase(aula, fontNormal));
                    table.AddCell(new iTextSharp.text.Phrase(edificio, fontNormal));
                    table.AddCell(new iTextSharp.text.Phrase(inicio, fontNormal));
                    table.AddCell(new iTextSharp.text.Phrase(fin, fontNormal));
                    table.AddCell(new iTextSharp.text.Phrase(devolucion, fontNormal));
                    table.AddCell(new iTextSharp.text.Phrase(estado, fontNormal));
                    table.AddCell(new iTextSharp.text.Phrase(retraso, fontNormal));
                }

                doc.Add(table);
                doc.Close();

                byte[] pdfBytes = stream.ToArray();
                return File(pdfBytes, "application/pdf", $"ReporteLlaves_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
            }


        }
       
        public IActionResult DisponibilidadAulas()
        {
            var todosLosHorarios = _db.HorarioAcademico
                .Include(h => h.Aula) 
                .ToList();
            var todasLasAulas = _db.Aula.OrderBy(a => a.Codigo).ToList();
            ViewBag.TodosLosHorarios = todosLosHorarios;
            ViewBag.TodasLasAulas = todasLasAulas;

            return View();
        }
    }
}
