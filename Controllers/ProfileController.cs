using GestionLlaves.Data;
using GestionLlaves.Models.ViewModels.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GestionLlaves.Controllers
{
    public class ProfileController : Controller
    {
        private readonly AppDbContext _context;

        public ProfileController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UsuarioId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var usuario = await _context.Usuario
                .Include(u => u.Persona)
                .FirstOrDefaultAsync(u => u.Id == userId.Value);

            var viewModel = new ProfileViewModel
            {
                NombreCompleto = $"{usuario.Persona.Nombres} {usuario.Persona.PrimerApellido} {usuario.Persona.SegundoApellido}".Trim(),
                Email = usuario.Email,
                Telefono = usuario.Persona.Telefono ?? "No registrado",
                Rol = usuario.Rol
            };

            return View(viewModel);
        }
    }
}