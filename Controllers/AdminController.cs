using GestionLlaves.Data;
using GestionLlaves.Helpers;
using GestionLlaves.Models;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;



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
            ViewBag.LlavesPendientes = ContarLlavesPendientes();


            ViewBag.Email = HttpContext.Session.GetString("UsuarioEmail");
            return View();
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
     string tipo,
     string email,
     string rol)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Account");
            ViewBag.LlavesPendientes = ContarLlavesPendientes();


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
        public IActionResult Solicitudes()
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Account");
            ViewBag.LlavesPendientes = ContarLlavesPendientes();


            // 🔹 Mostrar préstamos activos recientes (últimas 24h)
            var solicitudes = _db.Prestamo
                .Include(p => p.Persona)
                    .ThenInclude(per => per.Usuario)
                .Include(p => p.Aula)
                    .ThenInclude(a => a.Edificio)
                .Where(p => p.EstadoPrestamo == "ACTIVO" && p.Estado)
                .OrderByDescending(p => p.FechaInicio)
                .ToList();

            return View(solicitudes);
        }

        [HttpPost]
        public IActionResult CambiarEstadoSolicitud(int id, string estado)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Account");
            ViewBag.LlavesPendientes = ContarLlavesPendientes();


            var solicitud = _db.Prestamo.FirstOrDefault(p => p.Id == id);
            if (solicitud != null)
            {
                solicitud.EstadoPrestamo = estado.ToUpper();
                solicitud.UltimaMod = DateTime.Now;
                _db.SaveChanges();
            }

            return RedirectToAction("Solicitudes");
        }
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
                .Where(p => p.EstadoPrestamo == "APROBADO" && p.FechaFinReal == null && p.Estado)
                .OrderBy(p => p.FechaInicio)
                .ToList();

            return View(llavesActivas);
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
                .Where(p => p.EstadoPrestamo == "ACTIVO" && p.FechaFinReal == null)
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

        [HttpGet]
        public IActionResult VerAulasDisponibles(string dia = "Lunes")
        {
            if (!EsAdmin())
                return RedirectToAction("Login", "Account");

            // Días válidos para evitar errores
            var diasValidos = new[] { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado" };
            if (!diasValidos.Contains(dia))
                dia = "Lunes";

            ViewBag.DiaSeleccionado = dia;

            // Rangos de horas (bloques)
            var bloques = new[]
            {
    new { HoraInicio = TimeSpan.Parse("07:35"), HoraFin = TimeSpan.Parse("08:25") },
    new { HoraInicio = TimeSpan.Parse("08:35"), HoraFin = TimeSpan.Parse("09:25") },
    new { HoraInicio = TimeSpan.Parse("09:25"), HoraFin = TimeSpan.Parse("10:15") },
    new { HoraInicio = TimeSpan.Parse("10:25"), HoraFin = TimeSpan.Parse("11:15") },
    new { HoraInicio = TimeSpan.Parse("11:15"), HoraFin = TimeSpan.Parse("12:05") },
    new { HoraInicio = TimeSpan.Parse("12:15"), HoraFin = TimeSpan.Parse("13:05") },
    new { HoraInicio = TimeSpan.Parse("13:05"), HoraFin = TimeSpan.Parse("13:55") },
    new { HoraInicio = TimeSpan.Parse("14:05"), HoraFin = TimeSpan.Parse("14:55") },
    new { HoraInicio = TimeSpan.Parse("14:55"), HoraFin = TimeSpan.Parse("15:45") },
    new { HoraInicio = TimeSpan.Parse("15:55"), HoraFin = TimeSpan.Parse("16:45") },
    new { HoraInicio = TimeSpan.Parse("16:45"), HoraFin = TimeSpan.Parse("17:35") },
};

            // Obtener todas las aulas
            var aulas = _db.Aula.ToList();

            // Obtener horarios existentes (ocupados) para ese día
            var horariosOcupados = _db.HorarioAcademico
                .Where(h => EF.Property<bool>(h, dia)) // Día dinámico
                .Select(h => new
                {
                    h.Aula.Codigo,
                    h.Materia,
                    h.HoraInicio,
                    h.HoraFin,
                    Lunes = h.Lunes,
                    Martes = h.Martes,
                    Miercoles = h.Miercoles,
                    Jueves = h.Jueves,
                    Viernes = h.Viernes,
                    Sabado = h.Sabado
                })
                .ToList();

            // Generar lista de horarios vacíos
            var horariosVacios = new List<dynamic>();

            foreach (var aula in aulas)
            {
                foreach (var bloque in bloques)
                {
                    bool ocupado = horariosOcupados.Any(h =>
                        h.Codigo == aula.Codigo &&
                        ((TimeSpan)h.HoraInicio < bloque.HoraFin && (TimeSpan)h.HoraFin > bloque.HoraInicio)
                    );

                    if (!ocupado)
                    {
                        horariosVacios.Add(new
                        {
                            Aula = aula.Codigo,
                            Materia = "—",
                            HoraInicio = bloque.HoraInicio,
                            HoraFin = bloque.HoraFin,
                            Lunes = dia == "Lunes",
                            Martes = dia == "Martes",
                            Miercoles = dia == "Miércoles",
                            Jueves = dia == "Jueves",
                            Viernes = dia == "Viernes",
                            Sabado = dia == "Sábado"
                        });
                    }
                }
            }

            ViewBag.Horarios = horariosVacios;
            return View();
        }





    }
}
