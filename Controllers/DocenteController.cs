using GestionLlaves.Data;
using GestionLlaves.Models;
using GestionLlaves.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionLlaves.Controllers
{
    public class DocenteController : Controller
    {
        private readonly AppDbContext _db;
        public DocenteController(AppDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            // Verifica sesión
            var uid = HttpContext.Session.GetInt32("UsuarioId");
            if (uid == null) return RedirectToAction("Login", "Account");

            return View(); // -> Views/Docente/Index.cshtml
        }


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

        // ---------------- SP2-02: MI HORARIO (HOY) ----------------
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

        // ---------------- SP2-03: MI ESTADO ----------------
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

        // ---------------- SP2-01: NUEVA RESERVA ----------------
        [HttpGet]
        public IActionResult NuevaReserva()
        {
            return View(new ReservaNuevaVM
            {
                HoraInicio = TimeOnly.FromDateTime(DateTime.Now.AddMinutes(30)),
                HoraFin = TimeOnly.FromDateTime(DateTime.Now.AddHours(2))
            });
        }

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
                .Select(a => new ReservaNuevaVM.AulaOpcion
                {
                    Id = a.Id,
                    Nombre = $"Aula {a.Codigo}",
                    Edificio = a.Edificio!.Nombre,
                    Capacidad = a.Capacidad ?? 0
                }).ToListAsync();

            return PartialView("_AulasDisponibles", aulas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NuevaReserva(ReservaNuevaVM vm)
        {
            var uid = await GetUsuarioId();
            if (uid is null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid || vm.AulaId is null)
                return View(vm);

            var inicio = vm.Fecha.ToDateTime(vm.HoraInicio);
            var fin = vm.Fecha.ToDateTime(vm.HoraFin);

            bool solape = await _db.Prestamo.AnyAsync(p =>
                p.AulaId == vm.AulaId && p.FechaFinReal == null && p.FechaInicio < fin && inicio < p.FechaFinProgramada)
                || await _db.Reserva.AnyAsync(r =>
                r.AulaId == vm.AulaId && r.EstadoReserva == "APROBADA" && r.FechaInicio < fin && inicio < r.FechaFin);

            if (solape)
            {
                ModelState.AddModelError(string.Empty, "El aula seleccionada ya no está disponible.");
                return View(vm);
            }

            _db.Reserva.Add(new Reserva
            {
                SolicitanteId = uid.Value,
                AulaId = vm.AulaId.Value,
                FechaInicio = inicio,
                FechaFin = fin,
                Proposito = vm.Proposito,
                Justificacion = vm.Justificacion,
                EstadoReserva = "PENDIENTE"
            });

            await _db.SaveChangesAsync();

            TempData["ok"] = "Solicitud enviada. Recibirás notificaciones cuando sea aprobada o rechazada.";
            return RedirectToAction(nameof(MisReservas));
        }

        // ---------------- SP2-01: MIS RESERVAS ----------------
        public async Task<IActionResult> MisReservas()
        {
            var uid = await GetUsuarioId();
            if (uid is null) return RedirectToAction("Login", "Account");

            var list = await _db.Reserva
                .Include(r => r.Aula)
                .ThenInclude(a => a.Edificio)
                .Where(r => r.SolicitanteId == uid)
                .OrderByDescending(r => r.FechaInicio)
                .Select(r => new ReservaCardVM
                {
                    Id = r.Id,
                    Aula = $"Aula {r.Aula!.Codigo} • {r.Aula!.Edificio!.Nombre}",
                    Inicio = r.FechaInicio,
                    Fin = r.FechaFin,
                    Estado = r.EstadoReserva
                }).ToListAsync();

            return View(list);
        }
    }
}
