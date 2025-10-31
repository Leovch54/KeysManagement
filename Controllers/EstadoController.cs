using GestionLlaves.Data;
using GestionLlaves.Models;
using GestionLlaves.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionLlaves.Controllers
{
    public class EstadoController : Controller
    {
        private readonly AppDbContext _db;

        public EstadoController(AppDbContext db)
        {
            _db = db;
        }

        // ============================
        // 🧩 VISTA PRINCIPAL: MiEstado
        // ============================
        [HttpGet]
        public async Task<IActionResult> MiEstado()
        {
            var uid = await GetUsuarioId();
            if (uid is null)
                return RedirectToAction("Login", "Account");

            var ahora = DateTime.Now;

            // Buscar préstamo activo del docente
            var prestamoActivo = await _db.Prestamo
                .Include(p => p.Aula)
                    .ThenInclude(a => a.Edificio)
                .Include(p => p.HorarioAcademico)
                    .ThenInclude(h => h.Materia)
                .Where(p => p.PersonaId == uid && p.EstadoPrestamo == "ACTIVO" && p.Estado)
                .OrderByDescending(p => p.FechaInicio)
                .FirstOrDefaultAsync();

            // Si no tiene préstamo activo, devolver vista vacía
            if (prestamoActivo == null)
            {
                var vmVacio = new EstadoViewModel
                {
                    AulaNombre = "—",
                    MateriaNombre = "Sin préstamo activo",
                    HoraInicio = DateTime.Now,
                    HoraFin = DateTime.Now,
                    TiempoRestante = TimeSpan.Zero,
                    PorcentajeTranscurrido = 0,
                    TotalPrestamos = await _db.Prestamo.CountAsync(p => p.PersonaId == uid),
                    DevolucionesATiempo = await _db.Prestamo.CountAsync(p =>
                        p.PersonaId == uid &&
                        p.FechaFinReal != null &&
                        p.FechaFinReal <= p.FechaFinProgramada),
                    PrestamosSinRetraso = 0,
                    PromedioUso = TimeSpan.Zero,
                    RetrasoPromedio = TimeSpan.Zero,
                    RachaDias = 0
                };

                return View("MiEstado", vmVacio);
            }

            // 🔹 Calcular tiempos
            var tiempoRestante = prestamoActivo.FechaFinProgramada - ahora;
            if (tiempoRestante < TimeSpan.Zero)
                tiempoRestante = TimeSpan.Zero;

            var duracionTotal = prestamoActivo.FechaFinProgramada - prestamoActivo.FechaInicio;
            double porcentajeTranscurrido = duracionTotal.TotalSeconds > 0
                ? ((duracionTotal.TotalSeconds - tiempoRestante.TotalSeconds) / duracionTotal.TotalSeconds) * 100
                : 0;

            // 🔹 Cálculo de estadísticas generales del docente
            var prestamosDocente = await _db.Prestamo
                .Where(p => p.PersonaId == uid)
                .ToListAsync();

            var devolucionesATiempo = prestamosDocente
                .Count(p => p.FechaFinReal != null && p.FechaFinReal <= p.FechaFinProgramada);

            var prestamosConRetraso = prestamosDocente
                .Where(p => p.FechaFinReal != null && p.FechaFinReal > p.FechaFinProgramada)
                .ToList();

            var retrasoPromedio = prestamosConRetraso.Any()
                ? TimeSpan.FromTicks((long)prestamosConRetraso
                    .Average(p => (p.FechaFinReal!.Value - p.FechaFinProgramada).Ticks))
                : TimeSpan.Zero;

            var promedioUso = prestamosDocente.Any()
                ? TimeSpan.FromTicks((long)prestamosDocente
                    .Average(p => (p.FechaFinProgramada - p.FechaInicio).Ticks))
                : TimeSpan.Zero;

            var rachaDias = 0;
            foreach (var p in prestamosDocente.OrderByDescending(p => p.FechaInicio))
            {
                if (p.FechaFinReal != null && p.FechaFinReal <= p.FechaFinProgramada)
                    rachaDias++;
                else
                    break;
            }

            var vm = new EstadoViewModel
            {
                AulaNombre = prestamoActivo.Aula?.Codigo ?? "N/A",
                MateriaNombre = prestamoActivo.HorarioAcademico?.Materia?.Nombre ?? "Préstamo actual",
                HoraInicio = prestamoActivo.FechaInicio,
                HoraFin = prestamoActivo.FechaFinProgramada,
                TiempoRestante = tiempoRestante,
                PorcentajeTranscurrido = porcentajeTranscurrido,
                TotalPrestamos = prestamosDocente.Count,
                DevolucionesATiempo = devolucionesATiempo,
                PrestamosSinRetraso = devolucionesATiempo,
                PromedioUso = promedioUso,
                RetrasoPromedio = retrasoPromedio,
                RachaDias = rachaDias
            };

            return View("MiEstado", vm);
        }

        // ============================
        // 🗝️ ACCIÓN: Marcar como devuelto
        // ============================
        [HttpPost]
        public async Task<IActionResult> MarcarDevuelto()
        {
            var uid = await GetUsuarioId();
            if (uid is null)
                return Json(new { ok = false, error = "Sesión expirada" });

            // Buscar préstamo activo del usuario
            var prestamo = await _db.Prestamo
                .Where(p => p.PersonaId == uid && p.EstadoPrestamo == "ACTIVO" && p.FechaFinReal == null)
                .FirstOrDefaultAsync();

            if (prestamo == null)
                return Json(new { ok = false, error = "No hay préstamo activo para devolver." });

            // Marcar como devuelto
            prestamo.FechaFinReal = DateTime.Now;
            prestamo.EstadoPrestamo = "DEVUELTO";
            prestamo.Estado = false;

            await _db.SaveChangesAsync();

            return Json(new { ok = true });
        }

        // ============================
        // 🔍 MÉTODO AUXILIAR: Obtener UsuarioId
        // ============================
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
    }
}
