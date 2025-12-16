using GestionLlaves.Data;
using GestionLlaves.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionLlaves.Controllers
{
    public class NotificacionController : Controller // Sin 's'
    {
        private readonly AppDbContext _context;

        public NotificacionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Notificacion/Index
        public async Task<IActionResult> Index()
        {
            var userId = await GetUsuarioId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var notificaciones = await _context.Notificacion
                .Include(n => n.HorarioAcademico)
                    .ThenInclude(h => h.Materia)
                .Include(n => n.Prestamo)
                    .ThenInclude(p => p.Aula)
                .Include(n => n.Reserva)
                    .ThenInclude(r => r.Aula)
                .Where(n => n.UsuarioId == userId && n.Estado)
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync();

            return View(notificaciones);
        }

        // POST: Notificacion/MarcarLeida/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarLeida(int id)
        {
            var userId = await GetUsuarioId();
            if (userId == null)
                return Json(new { ok = false, error = "Sesión expirada" });

            var notificacion = await _context.Notificacion
                .FirstOrDefaultAsync(n => n.Id == id && n.UsuarioId == userId);

            if (notificacion == null)
                return Json(new { ok = false, error = "Notificación no encontrada" });

            notificacion.Leida = true;
            notificacion.FechaLectura = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new { ok = true });
        }

        // POST: Notificacion/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var userId = await GetUsuarioId();
            if (userId == null)
                return Json(new { ok = false, error = "Sesión expirada" });

            var notificacion = await _context.Notificacion
                .FirstOrDefaultAsync(n => n.Id == id && n.UsuarioId == userId);

            if (notificacion == null)
                return Json(new { ok = false, error = "Notificación no encontrada" });

            _context.Notificacion.Remove(notificacion);
            await _context.SaveChangesAsync();

            return Json(new { ok = true });
        }

        private async Task<int?> GetUsuarioId()
        {
            int? id = HttpContext.Session.GetInt32("UsuarioId");
            if (id.HasValue) return id;

            var email = HttpContext.Session.GetString("UsuarioEmail");
            if (string.IsNullOrWhiteSpace(email)) return null;

            var usuario = await _context.Usuario
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);

            if (usuario != null)
            {
                HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
                return usuario.Id;
            }

            return null;
        }
    }
}