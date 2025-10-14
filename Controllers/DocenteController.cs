using GestionLlaves.Data;
using GestionLlaves.Models;
using GestionLlaves.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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
        public async Task<IActionResult> Solicitar()
        {
            var uid = await GetUsuarioId();
            if (uid is null) return RedirectToAction("Login", "Account");

            var ahora = DateTime.Now;
            var hoy = DateOnly.FromDateTime(ahora);

            // 1️⃣ Reserva actual o próxima (solo la más cercana)
            var reservaActiva = await _db.Reserva
                .Include(r => r.Aula).ThenInclude(a => a.Edificio)
                .Where(r => r.SolicitanteId == uid &&
                            (r.EstadoReserva == "APROBADA" || r.EstadoReserva == "ACTIVO") &&
                            r.FechaInicio.Date == hoy.ToDateTime(TimeOnly.MinValue).Date)
                .OrderBy(r => r.FechaInicio)
                .FirstOrDefaultAsync();

            // 2️⃣ Préstamos activos (ya con llave entregada)
            var prestamosActivos = await _db.Prestamo
                .Include(p => p.Aula).ThenInclude(a => a.Edificio)
                .Where(p => p.PersonaId == uid && p.FechaFinReal == null)
                .OrderBy(p => p.FechaInicio)
                .ToListAsync();

            // 3️⃣ Próxima clase o reserva
            var proximo = await _db.HorariosAcademico
                .Include(h => h.Materia)
                .Include(h => h.Aula)
                .Where(h => h.DocenteId == uid && h.EstadoHorario)
                .OrderBy(h => h.HoraInicio)
                .FirstOrDefaultAsync();

            var vm = new
            {
                NombreDocente = (await _db.Persona.FirstOrDefaultAsync(p => p.Id == uid))?.Nombres ?? "Docente",
                ReservaActiva = reservaActiva != null ? new
                {
                    Aula = $"Aula {reservaActiva.Aula!.Codigo} • {reservaActiva.Aula.Edificio!.Nombre}",
                    Inicio = reservaActiva.FechaInicio,
                    Fin = reservaActiva.FechaFin,
                    Estado = reservaActiva.EstadoReserva
                } : null,
                Prestamos = prestamosActivos.Select(p => new
                {
                    Aula = $"Aula {p.Aula!.Codigo} • {p.Aula.Edificio!.Nombre}",
                    Inicio = p.FechaInicio,
                    Fin = p.FechaFinProgramada
                }).ToList(),
                Proximo = proximo != null ? new
                {
                    Materia = proximo.Materia!.Nombre,
                    Aula = $"Aula {proximo.Aula!.Codigo}",
                    HoraInicio = proximo.HoraInicio,
                    HoraFin = proximo.HoraFin
                } : null
            };

            return View(vm);
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