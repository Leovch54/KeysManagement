using GestionLlaves.Data;
using GestionLlaves.Models;
using GestionLlaves.Models.ViewModels;
using GestionLlaves.Models.ViewModels.Solicitar;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using GestionLlaves.Models.ViewModels;

namespace GestionLlaves.Controllers
{
    public class DocenteController : Controller
    {
        private readonly AppDbContext _db;
        public DocenteController(AppDbContext db)
        {
            _db = db;
        }

        // ------------------- INDEX / DASHBOARD -------------------
        public IActionResult Index()
        {
            var uid = HttpContext.Session.GetInt32("UsuarioId");
            if (uid == null) return RedirectToAction("Login", "Account");
            return View();
        }
        public IActionResult Dashboard() => View();
        public IActionResult Agenda()
        {
            return RedirectToAction("Horario");
        }

        // Solicitar → Redirige a la reserva de aula
        // ---------------- SP2-04: SOLICITAR LLAVE / DASHBOARD ----------------
        [HttpGet]
        public async Task<IActionResult> Solicitar()
        {
            Console.WriteLine("Método Solicitar ejecutado");

            var usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            Console.WriteLine($"Usuario ID desde sesión: {usuarioId}");

            if (usuarioId == 0)
            {
                Console.WriteLine("Usuario no autenticado, redirigiendo a login");
                return RedirectToAction("Login", "Account");
            }

            var usuario = await _db.Usuario
                .Include(u => u.Persona)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null)
            {
                return NotFound();
            }

            var viewModel = new SolicitarLlaveViewModel
            {
                NombreDocente = usuario.Persona?.Nombres ?? "Docente",
                FechaActual = DateTime.Now
            };

            var ahora = DateTime.Now;
            var hoy = DateTime.Today;

            // 1. SOLICITAR CARD - Prioridad: Clase actual > Reserva actual > Sin nada
            var periodoActivo = await _db.PeriodosAcademico
                .Where(p => p.Activo && p.Estado)
                .FirstOrDefaultAsync();

            ItemSolicitarViewModel? itemSolicitar = null;

            if (periodoActivo != null)
            {
                var diaSemanaHoy = ObtenerDiaSemana(hoy);

                // Buscar clase actual (dentro del horario)
                // Buscar clase actual (dentro del horario, incluso si cruza medianoche)
                var todosLosHorariosHoy = await _db.HorariosAcademico
                    .Include(h => h.Materia)
                    .Include(h => h.Aula)
                        .ThenInclude(a => a.Edificio)
                    .Where(h => h.DocenteId == usuarioId
                        && h.PeriodoAcademicoId == periodoActivo.Id
                        && h.EstadoHorario
                        && h.Estado)
                    .ToListAsync();

                var claseActual = todosLosHorariosHoy
                    .Where(h => EsDiaActivo(h, diaSemanaHoy))
                    .Where(h =>
                        (ahora.TimeOfDay >= h.HoraInicio && ahora.TimeOfDay <= h.HoraFin) // horario normal
                        || (h.HoraInicio > h.HoraFin && (ahora.TimeOfDay >= h.HoraInicio || ahora.TimeOfDay <= h.HoraFin)) // cruza medianoche
                    )
                    .OrderBy(h => h.HoraInicio)
                    .FirstOrDefault();

                if (claseActual != null)
                {
                    // Verificar que no tenga un préstamo activo para esta clase
                    var tienePrestamo = await _db.Prestamo
                        .AnyAsync(p => p.PersonaId == usuarioId
                            && p.HorarioAcademicoId == claseActual.Id
                            && p.EstadoPrestamo == "ACTIVO"
                            && p.Estado);

                    if (!tienePrestamo)
                    {
                        itemSolicitar = new ItemSolicitarViewModel
                        {
                            Tipo = "CLASE",
                            HorarioAcademicoId = claseActual.Id,
                            AulaId = claseActual.AulaId,
                            Titulo = claseActual.Materia?.Nombre ?? "Clase",
                            Subtitulo = $"{claseActual.Aula?.Codigo} - {claseActual.Aula?.Edificio?.Nombre}",
                            HoraInicio = hoy.Add(claseActual.HoraInicio),
                            HoraFin = hoy.Add(claseActual.HoraFin),
                            TipoPrestamo = "REGULAR"
                        };
                    }
                }
            }

            // Si no hay clase actual, buscar reserva aprobada actual
            if (itemSolicitar == null)
            {
                var reservaActual = await _db.Reserva
                    .Include(r => r.Aula)
                        .ThenInclude(a => a.Edificio)
                    .Where(r => r.SolicitanteId == usuarioId
                        && r.EstadoReserva == "APROBADA"
                        && r.FechaInicio <= ahora
                        && r.FechaFin >= ahora
                        && r.Estado == true
                        && !_db.Prestamo.Any(p => p.ReservaId == r.Id && p.EstadoPrestamo == "ACTIVO"))
                    .OrderBy(r => r.FechaInicio)
                    .FirstOrDefaultAsync();

                if (reservaActual != null)
                {
                    itemSolicitar = new ItemSolicitarViewModel
                    {
                        Tipo = "RESERVA",
                        ReservaId = reservaActual.Id,
                        AulaId = reservaActual.AulaId,
                        Titulo = reservaActual.Proposito ?? "Reserva Excepcional",
                        Subtitulo = $"{reservaActual.Aula?.Codigo} - {reservaActual.Aula?.Edificio?.Nombre}",
                        HoraInicio = reservaActual.FechaInicio,
                        HoraFin = reservaActual.FechaFin,
                        TipoPrestamo = "EXCEPCIONAL"
                    };
                }
            }

            viewModel.ItemSolicitar = itemSolicitar;

            // 2. ESTADO ACTUAL DE LLAVES - Máximo 2 items
            var estadoLlaves = new List<LlaveActivaViewModel>();

            // Primero: Préstamo en uso actual
            var prestamoEnUso = await _db.Prestamo
                .Include(p => p.Aula)
                    .ThenInclude(a => a.Edificio)
                .Include(p => p.HorarioAcademico)
                    .ThenInclude(h => h.Materia)
                .Include(p => p.Reserva)
                .Where(p => p.PersonaId == usuarioId
                    && p.EstadoPrestamo == "ACTIVO"
                    && p.FechaInicio <= ahora
                    && p.FechaFinProgramada >= ahora
                    && p.Estado)
                .OrderBy(p => p.FechaInicio)
                .FirstOrDefaultAsync();

            if (prestamoEnUso != null)
            {
                estadoLlaves.Add(new LlaveActivaViewModel
                {
                    PrestamoId = prestamoEnUso.Id,
                    CodigoAula = prestamoEnUso.Aula?.Codigo ?? "N/A",
                    NombreEdificio = prestamoEnUso.Aula?.Edificio?.Nombre ?? "N/A",
                    Descripcion = prestamoEnUso.HorarioAcademico?.Materia?.Nombre
                        ?? prestamoEnUso.Reserva?.Proposito
                        ?? "Préstamo",
                    HoraInicio = prestamoEnUso.FechaInicio,
                    HoraFin = prestamoEnUso.FechaFinProgramada,
                    EstadoPrestamo = "ACTIVO",
                    EsEnUso = true
                });
            }

            // Segundo: Siguiente préstamo del día (dentro de las próximas 35 horas)
            if (estadoLlaves.Count < 2)
            {
                var limiteProximo = ahora.AddHours(35);
                var finDelDia = hoy.AddDays(1).AddSeconds(-1);
                var limiteReal = limiteProximo > finDelDia ? finDelDia : limiteProximo;

                var siguientePrestamo = await _db.Prestamo
                    .Include(p => p.Aula)
                        .ThenInclude(a => a.Edificio)
                    .Include(p => p.HorarioAcademico)
                        .ThenInclude(h => h.Materia)
                    .Include(p => p.Reserva)
                    .Where(p => p.PersonaId == usuarioId
                        && p.EstadoPrestamo == "ACTIVO"
                        && p.FechaInicio > ahora
                        && p.FechaInicio <= limiteReal
                        && p.Estado)
                    .OrderBy(p => p.FechaInicio)
                    .FirstOrDefaultAsync();

                if (siguientePrestamo != null)
                {
                    estadoLlaves.Add(new LlaveActivaViewModel
                    {
                        PrestamoId = siguientePrestamo.Id,
                        CodigoAula = siguientePrestamo.Aula?.Codigo ?? "N/A",
                        NombreEdificio = siguientePrestamo.Aula?.Edificio?.Nombre ?? "N/A",
                        Descripcion = siguientePrestamo.HorarioAcademico?.Materia?.Nombre
                            ?? siguientePrestamo.Reserva?.Proposito
                            ?? "Préstamo",
                        HoraInicio = siguientePrestamo.FechaInicio,
                        HoraFin = siguientePrestamo.FechaFinProgramada,
                        EstadoPrestamo = "ACTIVO",
                        EsEnUso = false
                    });
                }
            }

            viewModel.LlavesActivas = estadoLlaves;

            // 3. PRÓXIMAS CLASES - Solo las siguientes 3 clases
            if (periodoActivo != null)
            {
                var diaSemanaHoy = ObtenerDiaSemana(hoy);
                var maniana = hoy.AddDays(1);
                var diaSemanaManiana = ObtenerDiaSemana(maniana);

                var todosLosHorarios = await _db.HorariosAcademico
                    .Include(h => h.Materia)
                    .Include(h => h.Aula)
                    .Where(h => h.DocenteId == usuarioId
                        && h.PeriodoAcademicoId == periodoActivo.Id
                        && h.EstadoHorario
                        && h.Estado)
                    .ToListAsync();

                var horariosHoy = todosLosHorarios
                    .Where(h => EsDiaActivo(h, diaSemanaHoy))
                    .Where(h => ahora.TimeOfDay < h.HoraFin) // Solo clases futuras de hoy
                    .ToList();

                var horariosManiana = todosLosHorarios
                    .Where(h => EsDiaActivo(h, diaSemanaManiana))
                    .ToList();

                var clasesHoy = horariosHoy.Select(h => new ProximaClaseViewModel
                {
                    HorarioAcademicoId = h.Id,
                    NombreMateria = h.Materia?.Nombre ?? "N/A",
                    CodigoAula = h.Aula?.Codigo ?? "N/A",
                    HoraInicio = h.HoraInicio,
                    HoraFin = h.HoraFin,
                    FechaClase = hoy
                }).ToList();

                var clasesManiana = horariosManiana.Select(h => new ProximaClaseViewModel
                {
                    HorarioAcademicoId = h.Id,
                    NombreMateria = h.Materia?.Nombre ?? "N/A",
                    CodigoAula = h.Aula?.Codigo ?? "N/A",
                    HoraInicio = h.HoraInicio,
                    HoraFin = h.HoraFin,
                    FechaClase = maniana
                }).ToList();

                viewModel.ProximasClases = clasesHoy.Concat(clasesManiana)
                    .OrderBy(c => c.FechaClase)
                    .ThenBy(c => c.HoraInicio)
                    .Take(3) // Solo las próximas 3 clases
                    .ToList();
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SolicitarLlave([FromBody] SolicitarLlaveRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Datos inválidos" });
            }

            var usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

            if (usuarioId == 0)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            try
            {
                // Validar que no tenga un préstamo activo para la misma aula
                var prestamoExistente = await _db.Prestamo
                    .AnyAsync(p => p.PersonaId == usuarioId
                        && p.AulaId == request.AulaId
                        && p.EstadoPrestamo == "ACTIVO"
                        && p.Estado);

                if (prestamoExistente)
                {
                    return Json(new { success = false, message = "Ya tienes un préstamo activo para esta aula" });
                }

                DateTime fechaInicio = DateTime.Now;
                DateTime fechaFin;

                // Determinar fecha fin según el tipo
               

                // 🔹 Determinar fecha fin según el tipo de préstamo
                if (request.Tipo == "EXCEPCIONAL" && request.ReservaId.HasValue)
                {
                    var reserva = await _db.Reserva.FindAsync(request.ReservaId.Value);
                    if (reserva == null || reserva.EstadoReserva != "APROBADA")
                    {
                        return Json(new { success = false, message = "Reserva no encontrada o no aprobada" });
                    }
                    fechaFin = reserva.FechaFin;
                }
                else if (request.HorarioAcademicoId.HasValue)
                {
                    var horario = await _db.HorariosAcademico.FindAsync(request.HorarioAcademicoId.Value);
                    if (horario == null)
                    {
                        return Json(new { success = false, message = "Horario no encontrado" });
                    }

                    var hoy = DateTime.Today;
                    fechaFin = hoy.Add(horario.HoraFin);
                }
                else
                {
                    // 🕒 Si no viene ni reserva ni horario → temporizador de 1 h 45 min
                    fechaFin = DateTime.Now.AddHours(1).AddMinutes(45);
                }


                // Crear el préstamo
                var prestamo = new Prestamo
                {
                    PersonaId = usuarioId,
                    AulaId = request.AulaId,
                    ReservaId = request.ReservaId,
                    HorarioAcademicoId = request.HorarioAcademicoId,
                    FechaInicio = fechaInicio,
                    FechaFinProgramada = fechaFin,
                    Tipo = request.Tipo,
                    EstadoPrestamo = "ACTIVO",
                    Estado = true,
                    FechaCreacion = DateTime.Now,
                    CreadoModPor = usuarioId
                };

                _db.Prestamo.Add(prestamo);
                await _db.SaveChangesAsync();

                return Json(new { success = true, message = "Llave solicitada exitosamente", prestamoId = prestamo.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al procesar la solicitud: {ex.Message}" });
            }
        }

        // Métodos auxiliares
        private string ObtenerDiaSemana(DateTime fecha)
        {
            return fecha.DayOfWeek switch
            {
                DayOfWeek.Monday => "Lunes",
                DayOfWeek.Tuesday => "Martes",
                DayOfWeek.Wednesday => "Miercoles",
                DayOfWeek.Thursday => "Jueves",
                DayOfWeek.Friday => "Viernes",
                DayOfWeek.Saturday => "Sabado",
                _ => ""
            };
        }

        private bool EsDiaActivo(HorarioAcademico horario, string dia)
        {
            return dia switch
            {
                "Lunes" => horario.Lunes,
                "Martes" => horario.Martes,
                "Miercoles" => horario.Miercoles,
                "Jueves" => horario.Jueves,
                "Viernes" => horario.Viernes,
                "Sabado" => horario.Sabado,
                _ => false
            };
        }

        // ------------------- HELPERS -------------------
        private async Task<int?> GetUsuarioId()
        {
            int? id = HttpContext.Session.GetInt32("UsuarioId");
            if (id.HasValue) return id;

            var email = HttpContext.Session.GetString("UsuarioEmail");
            if (string.IsNullOrWhiteSpace(email)) return null;

            var usuario = await _db.Usuario.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
            if (usuario != null)
            {
                HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
                return usuario.Id;
            }
            return null;
        }

        // ------------------- SP2-02: MI HORARIO (HOY) -------------------
        public async Task<IActionResult> Horario()
        {
            var uid = await GetUsuarioId();
            if (uid is null) return RedirectToAction("Login", "Account");

            var hoy = DateTime.Now;
            var dow = hoy.DayOfWeek;

            var query = _db.HorariosAcademico
                .Include(h => h.Materia)
                .Include(h => h.Aula)
                .Where(h => h.DocenteId == uid && h.EstadoHorario == true);

            query = dow switch
            {
                DayOfWeek.Monday => query.Where(h => h.Lunes),
                DayOfWeek.Tuesday => query.Where(h => h.Martes),
                DayOfWeek.Wednesday => query.Where(h => h.Miercoles),
                DayOfWeek.Thursday => query.Where(h => h.Jueves),
                DayOfWeek.Friday => query.Where(h => h.Viernes),
                DayOfWeek.Saturday => query.Where(h => h.Sabado),
                _ => query.Where(h => false)
            };

            var items = await query.OrderBy(h => h.HoraInicio).Select(h => new HorarioItemVM
            {
                HorarioId = h.Id,
                Materia = h.Materia!.Nombre,
                Aula = $"Aula {h.Aula!.Codigo}",
                Inicio = h.HoraInicio,
                Fin = h.HoraFin,
                EnCurso = h.HoraInicio <= hoy.TimeOfDay && hoy.TimeOfDay <= h.HoraFin
            }).ToListAsync();

            ViewData["FechaLabel"] = hoy.ToString("dddd, dd 'de' MMMM", new System.Globalization.CultureInfo("es-BO"));
            return View(items);
        }

        // ------------------- SP2-03: MI ESTADO -------------------
        public async Task<IActionResult> Estado()
        {
            var uid = await GetUsuarioId();
            if (uid is null) return RedirectToAction("Login", "Account");

            var ahora = DateTime.Now;

            var activo = await _db.Prestamo
                .Include(p => p.Aula)
                .Where(p => p.PersonaId == uid && p.FechaFinReal == null)
                .OrderByDescending(p => p.FechaInicio)
                .FirstOrDefaultAsync();

            var vm = new EstadoVM();

            if (activo != null)
            {
                vm.TienePrestamoActivo = true;
                vm.AulaActual = $"Aula {activo.Aula!.Codigo}";
                vm.FinProgramado = activo.FechaFinProgramada;
                vm.TiempoRestante = activo.FechaFinProgramada - ahora;
            }

            var historico = await _db.Prestamo
                .Include(p => p.Aula)
                .Where(p => p.PersonaId == uid)
                .OrderByDescending(p => p.FechaInicio)
                .Take(10)
                .ToListAsync();

            vm.TotalPrestamos = await _db.Prestamo.CountAsync(p => p.PersonaId == uid);
            vm.DevolucionesATiempo = await _db.Prestamo.CountAsync(p => p.PersonaId == uid &&
                p.FechaFinReal != null && p.FechaFinReal <= p.FechaFinProgramada);

            int racha = 0;
            foreach (var p in historico.Where(p => p.FechaFinReal != null))
            {
                bool ok = p.FechaFinReal!.Value <= p.FechaFinProgramada;
                if (ok) racha++; else break;
            }
            vm.RachaATiempo = racha;

            vm.Historial = historico.Select(p => new EstadoVM.HistorialItem
            {
                Fecha = p.FechaInicio,
                Aula = $"Aula {p.Aula!.Codigo}",
                ATiempo = p.FechaFinReal != null && p.FechaFinReal <= p.FechaFinProgramada
            }).ToList();

            return View(vm);
        }

        // ------------------- SP2-01: NUEVA RESERVA (GET) -------------------
        [HttpGet]
        public IActionResult NuevaReserva()
        {
            return View(new ReservaNuevaVM
            {
                HoraInicio = TimeOnly.FromDateTime(DateTime.Now.AddMinutes(30)),
                HoraFin = TimeOnly.FromDateTime(DateTime.Now.AddHours(2))
            });
        }

        // ------------------- AULAS DISPONIBLES (JSON) -------------------
        [HttpGet]
        public async Task<IActionResult> AulasDisponibles(DateOnly fecha, TimeOnly horaInicio, TimeOnly horaFin)
        {
            var inicio = fecha.ToDateTime(horaInicio);
            var fin = fecha.ToDateTime(horaFin);

            var ocupadasPrestamo = _db.Prestamo
                .Where(p => p.FechaFinReal == null && p.FechaInicio < fin && inicio < p.FechaFinProgramada)
                .Select(p => p.AulaId);

            var ocupadasReserva = _db.Reserva
                .Where(r => r.EstadoReserva == "APROBADA" && r.FechaInicio < fin && inicio < r.FechaFin)
                .Select(r => r.AulaId);

            var ocupadas = ocupadasPrestamo.Union(ocupadasReserva);

            var aulas = await _db.Aula
                .Include(a => a.Edificio)
                .Where(a => a.Estado == true && !ocupadas.Contains(a.Id))
                .OrderBy(a => a.Edificio!.Nombre)
                .ThenBy(a => a.Codigo)
                .Select(a => new
                {
                    id = a.Id,
                    nombre = $"Aula {a.Codigo}",
                    edificio = a.Edificio!.Nombre,
                    capacidad = a.Capacidad ?? 0
                })
                .ToListAsync();

            return Json(aulas);
        }

        // ------------------- SP2-01: NUEVA RESERVA (POST JSON) - CORREGIDO -------------------
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> NuevaReserva([FromBody] JsonElement body)
        {
            var uid = await GetUsuarioId();
            if (uid is null)
                return Json(new { ok = false, error = "Sesión expirada" });

            try
            {
                int aulaId = body.GetProperty("AulaId").GetInt32();
                string fechaStr = body.GetProperty("Fecha").GetString() ?? "";
                string horaInicioStr = body.GetProperty("HoraInicio").GetString() ?? "";
                string horaFinStr = body.GetProperty("HoraFin").GetString() ?? "";
                string proposito = body.TryGetProperty("Proposito", out var p) ? p.GetString() ?? "" : "Sin propósito";
                string justificacion = body.TryGetProperty("Justificacion", out var j) ? j.GetString() ?? "" : "Sin justificación";

                var fecha = DateOnly.Parse(fechaStr);
                var horaInicio = TimeOnly.Parse(horaInicioStr);
                var horaFin = TimeOnly.Parse(horaFinStr);
                var inicio = fecha.ToDateTime(horaInicio);
                var fin = fecha.ToDateTime(horaFin);

                // ✅ Validaciones de horario
                if (fin <= inicio)
                    return Json(new { ok = false, error = "La hora fin debe ser mayor que la hora inicio." });

                if (inicio < DateTime.Now)
                    return Json(new { ok = false, error = "No se puede reservar en una fecha u hora pasada." });

                if ((fin - inicio).TotalHours > 4)
                    return Json(new { ok = false, error = "La reserva no puede exceder 4 horas." });

                // 🔹 Verificar disponibilidad
                bool ocupado = await _db.Prestamo.AnyAsync(p =>
                    p.AulaId == aulaId && p.FechaFinReal == null &&
                    p.FechaInicio < fin && inicio < p.FechaFinProgramada)
                    || await _db.Reserva.AnyAsync(r =>
                    r.AulaId == aulaId && r.EstadoReserva == "APROBADA" &&
                    r.FechaInicio < fin && inicio < r.FechaFin);

                if (ocupado)
                    return Json(new { ok = false, error = "El aula ya no está disponible." });

                // 🔹 Guardar
                var prestamo = new Prestamo
                {
                    PersonaId = uid.Value,
                    AulaId = aulaId,
                    FechaInicio = inicio,
                    FechaFinProgramada = fin,
                    FechaFinReal = null,
                    Tipo = "RESERVA",
                    EstadoPrestamo = "ACTIVO",
                    Estado = true,
                    FechaCreacion = DateTime.Now,
                    CreadoModPor = uid.Value
                };

                _db.Prestamo.Add(prestamo);
                await _db.SaveChangesAsync();

                return Json(new { ok = true, redirect = Url.Action(nameof(MisReservas)) });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = "Error al guardar reserva", detalle = ex.Message });
            }
        }


        // ------------------- SP2-01: MIS RESERVAS -------------------
        public async Task<IActionResult> MisReservas()
        {
            var uid = await GetUsuarioId();
            if (uid is null) return RedirectToAction("Login", "Account");

            var reservas = await _db.Reserva
                .Include(r => r.Aula).ThenInclude(a => a.Edificio)
                .Where(r => r.SolicitanteId == uid)
                .Select(r => new ReservaCardVM
                {
                    Id = r.Id,
                    Aula = $"Aula {r.Aula!.Codigo} • {r.Aula!.Edificio!.Nombre}",
                    Inicio = r.FechaInicio,
                    Fin = r.FechaFin,
                    Estado = r.EstadoReserva
                })
                .ToListAsync();

            var prestamosComoReserva = await _db.Prestamo
                .Include(p => p.Aula).ThenInclude(a => a.Edificio)
                .Where(p => p.PersonaId == uid && p.Tipo == "RESERVA")
                .Select(p => new ReservaCardVM
                {
                    Id = p.Id,
                    Aula = $"Aula {p.Aula!.Codigo} • {p.Aula!.Edificio!.Nombre}",
                    Inicio = p.FechaInicio,
                    Fin = p.FechaFinProgramada,
                    Estado = p.EstadoPrestamo
                })
                .ToListAsync();

            var list = reservas.Concat(prestamosComoReserva)
                               .OrderByDescending(x => x.Inicio)
                               .ToList();

            return View(list);
        }
        [HttpPost]
        public async Task<IActionResult> CancelarReserva(int id)
        {
            try
            {
                var reserva = await _db.Reserva.FirstOrDefaultAsync(r => r.Id == id);
                if (reserva != null)
                {
                    // Si está en estado PENDIENTE o ACTIVO, se puede cancelar
                    if (reserva.EstadoReserva == "PENDIENTE" || reserva.EstadoReserva == "ACTIVO")
                    {
                        reserva.EstadoReserva = "CANCELADA";
                        reserva.MotivoRechazo = "Cancelada por el docente";
                        reserva.UltimaMod = DateTime.Now;
                        await _db.SaveChangesAsync();
                        return Json(new { ok = true });
                    }
                    else
                        return Json(new { ok = false, error = "Solo se pueden cancelar reservas activas o pendientes." });
                }

                return Json(new { ok = false, error = "Reserva no encontrada." });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = "Error al cancelar la reserva.", detalle = ex.Message });
            }
        }



    }

}