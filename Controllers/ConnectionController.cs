using GestionLlaves.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionLlaves.Controllers
{
    public class ConnectionController : Controller
    {
        private readonly AppDbContext _context;

        public ConnectionController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Prueba 1: Verificar que el contexto no sea nulo
                ViewBag.ContextStatus = _context != null ? "? Contexto inicializado" : "? Contexto es nulo";

                // Prueba 2: Intentar abrir la conexión
                var canConnect = await _context.Database.CanConnectAsync();
                ViewBag.ConnectionStatus = canConnect ? "? Conexión exitosa" : "? No se puede conectar";

                // Prueba 3: Obtener el nombre de la base de datos
                ViewBag.DatabaseName = _context.Database.GetDbConnection().Database;

                // Prueba 4: Contar registros en algunas tablas (si existen)
                ViewBag.EdificiosCount = await _context.Edificio.CountAsync();
                ViewBag.MateriasCount = await _context.Materia.CountAsync();
                ViewBag.PersonasCount = await _context.Persona.CountAsync();
                ViewBag.UsuariosCount = await _context.Usuario.CountAsync();
                ViewBag.AulasCount = await _context.Aula.CountAsync();

                ViewBag.TestStatus = "? TODAS LAS PRUEBAS PASARON";
            }
            catch (Exception ex)
            {
                ViewBag.TestStatus = "? ERROR EN LA CONEXIÓN";
                ViewBag.ErrorMessage = ex.Message;
                ViewBag.ErrorDetails = ex.InnerException?.Message ?? "No hay detalles adicionales";
            }

            return View();
        }

        public async Task<IActionResult> TestConnection()
        {
            var result = new
            {
                success = false,
                message = "",
                details = new Dictionary<string, object>()
            };

            try
            {
                // Verificar conexión
                var canConnect = await _context.Database.CanConnectAsync();

                if (canConnect)
                {
                    result = new
                    {
                        success = true,
                        message = "Conexión exitosa a la base de datos",
                        details = new Dictionary<string, object>
                        {
                            { "database", _context.Database.GetDbConnection().Database },
                            { "edificios", await _context.Edificio.CountAsync() },
                            { "materias", await _context.Materia.CountAsync() },
                            { "personas", await _context.Persona.CountAsync() },
                            { "usuarios", await _context.Usuario.CountAsync() },
                            { "aulas", await _context.Aula.CountAsync() }
                        }
                    };
                }
                else
                {
                    result = new
                    {
                        success = false,
                        message = "No se pudo conectar a la base de datos",
                        details = new Dictionary<string, object>()
                    };
                }
            }
            catch (Exception ex)
            {
                result = new
                {
                    success = false,
                    message = ex.Message,
                    details = new Dictionary<string, object>
                    {
                        { "innerException", ex.InnerException?.Message ?? "No hay detalles" },
                        { "stackTrace", ex.StackTrace ?? "" }
                    }
                };
            }

            return Json(result);
        }

    }
}
