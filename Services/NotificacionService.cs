using GestionLlaves.Data;
using GestionLlaves.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GestionLlaves.Services
{
    /// <summary>
    /// Servicio de lógica de negocio para la gestión de notificaciones automáticas
    /// </summary>
    public class NotificacionService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NotificacionService> _logger;

        public NotificacionService(AppDbContext context, ILogger<NotificacionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ==========================================
        // ENVÍO DE NOTIFICACIONES PROGRAMADAS
        // ==========================================

        /// <summary>
        /// Envía todas las notificaciones que ya llegaron a su fecha programada
        /// </summary>
        public async Task<int> EnviarNotificacionesProgramadasAsync()
        {
            try
            {
                var ahora = DateTime.Now;

                // Buscar notificaciones que:
                // 1. Ya llegó su hora de envío (FechaProgramada <= ahora)
                // 2. Aún no se han enviado (FechaEnvio == null)
                // 3. Están activas (Estado == true)
                var notificacionesPendientes = await _context.Notificacion
                    .Where(n =>
                        n.FechaProgramada.HasValue &&
                        n.FechaProgramada.Value <= ahora &&
                        n.FechaEnvio == null &&
                        n.Estado == true
                    )
                    .ToListAsync();

                if (!notificacionesPendientes.Any())
                {
                    _logger.LogInformation("No hay notificaciones programadas pendientes de envío.");
                    return 0;
                }

                // Marcar todas como enviadas
                foreach (var notificacion in notificacionesPendientes)
                {
                    notificacion.FechaEnvio = ahora;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    $"✓ {notificacionesPendientes.Count} notificaciones programadas enviadas correctamente."
                );

                return notificacionesPendientes.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificaciones programadas");
                return 0;
            }
        }

        // ==========================================
        // MONITOREO DE LLAVES NO DEVUELTAS
        // ==========================================

        /// <summary>
        /// Detecta préstamos vencidos y genera notificaciones de alerta
        /// </summary>
        public async Task<int> MonitorearLlavesNoDevueltasAsync()
        {
            try
            {
                var ahora = DateTime.Now;
                int notificacionesCreadas = 0;

                // Buscar préstamos activos que ya vencieron
                var prestamosVencidos = await _context.Prestamo
                    .Include(p => p.Persona)
                    .Include(p => p.Aula)
                        .ThenInclude(a => a.Edificio)
                    .Where(p =>
                        p.EstadoPrestamo == "ACTIVO" &&
                        p.FechaFinProgramada < ahora &&
                        p.Estado == true
                    )
                    .ToListAsync();

                if (!prestamosVencidos.Any())
                {
                    _logger.LogInformation("No hay llaves vencidas.");
                    return 0;
                }

                foreach (var prestamo in prestamosVencidos)
                {
                    // Actualizar estado del préstamo
                    prestamo.EstadoPrestamo = "VENCIDO";
                    prestamo.UltimaMod = ahora;

                    // Verificar si ya existe notificación de llave no devuelta para este préstamo
                    var notificacionExiste = await _context.Notificacion
                        .AnyAsync(n =>
                            n.PrestamoId == prestamo.Id &&
                            n.Tipo == "LLAVE_NO_DEVUELTA" &&
                            n.Estado == true
                        );

                    if (notificacionExiste)
                    {
                        _logger.LogInformation(
                            $"Ya existe notificación LLAVE_NO_DEVUELTA para préstamo {prestamo.Id}"
                        );
                        continue;
                    }

                    // Obtener usuario del docente
                    var usuario = await _context.Usuario
                        .FirstOrDefaultAsync(u => u.Id == prestamo.PersonaId);

                    if (usuario == null)
                    {
                        _logger.LogWarning(
                            $"No se encontró usuario para persona {prestamo.PersonaId} (préstamo {prestamo.Id})"
                        );
                        continue;
                    }

                    // Calcular tiempo de retraso
                    var tiempoVencido = ahora - prestamo.FechaFinProgramada;
                    var minutosVencidos = (int)tiempoVencido.TotalMinutes;

                    // Crear mensaje dinámico
                    string mensaje = $"La llave del Aula {prestamo.Aula.Codigo} debió ser devuelta a las " +
                                   $"{prestamo.FechaFinProgramada:HH:mm}. " +
                                   $"Retraso: {minutosVencidos} minutos. " +
                                   $"Por favor, devuélvela lo antes posible.";

                    // Crear notificación
                    var notificacion = new Notificacion
                    {
                        UsuarioId = usuario.Id,
                        Tipo = "LLAVE_NO_DEVUELTA",
                        Mensaje = mensaje,
                        Prioridad = "ALTA",
                        PrestamoId = prestamo.Id,
                        Leida = false,
                        FechaCreacion = ahora,
                        FechaProgramada = ahora, // Ya debía estar enviada
                        FechaEnvio = ahora,       // Se envía inmediatamente
                        Estado = true
                    };

                    _context.Notificacion.Add(notificacion);
                    notificacionesCreadas++;

                    _logger.LogInformation(
                        $"Notificación LLAVE_NO_DEVUELTA creada para préstamo {prestamo.Id} " +
                        $"(Usuario: {usuario.Email}, Aula: {prestamo.Aula.Codigo})"
                    );
                }

                await _context.SaveChangesAsync();

                if (notificacionesCreadas > 0)
                {
                    _logger.LogInformation(
                        $"✓ {notificacionesCreadas} notificaciones de llaves no devueltas creadas. " +
                        $"Total préstamos vencidos: {prestamosVencidos.Count}"
                    );
                }

                return notificacionesCreadas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al monitorear llaves no devueltas");
                return 0;
            }
        }

        // ==========================================
        // PRE-GENERACIÓN DE NOTIFICACIONES DE PRÓXIMA CLASE
        // ==========================================

        /// <summary>
        /// Genera notificaciones de "Próxima Clase" para el día siguiente
        /// Se ejecuta una vez al día (típicamente a las 00:00)
        /// </summary>
        public async Task<int> GenerarNotificacionesProximaClaseAsync()
        {
            try
            {
                var hoy = DateTime.Today;
                var manana = hoy.AddDays(1);
                int notificacionesCreadas = 0;

                // Determinar qué día de la semana es mañana
                var diaSemana = manana.DayOfWeek;

                // Obtener periodo académico activo
                var periodoActivo = await _context.PeriodoAcademico
                    .FirstOrDefaultAsync(p => p.Activo && p.Estado);

                if (periodoActivo == null)
                {
                    _logger.LogWarning("No hay periodo académico activo para generar notificaciones.");
                    return 0;
                }

                // Validar que mañana esté dentro del periodo académico
                if (manana < periodoActivo.FechaInicio || manana > periodoActivo.FechaFin)
                {
                    _logger.LogInformation(
                        $"La fecha {manana:dd/MM/yyyy} está fuera del periodo académico activo."
                    );
                    return 0;
                }

                // Construir query dinámico según el día de la semana
                var query = _context.HorarioAcademico
                    .Include(h => h.Materia)
                    .Include(h => h.Docente)
                    .Include(h => h.Aula)
                        .ThenInclude(a => a.Edificio)
                    .Where(h =>
                        h.PeriodoAcademicoId == periodoActivo.Id &&
                        h.EstadoHorario == true &&
                        h.Estado == true
                    );

                // Filtrar por día de la semana
                query = diaSemana switch
                {
                    DayOfWeek.Monday => query.Where(h => h.Lunes),
                    DayOfWeek.Tuesday => query.Where(h => h.Martes),
                    DayOfWeek.Wednesday => query.Where(h => h.Miercoles),
                    DayOfWeek.Thursday => query.Where(h => h.Jueves),
                    DayOfWeek.Friday => query.Where(h => h.Viernes),
                    DayOfWeek.Saturday => query.Where(h => h.Sabado),
                    _ => query.Where(h => false) // Domingo o día inválido
                };

                var horariosManana = await query.ToListAsync();

                if (!horariosManana.Any())
                {
                    _logger.LogInformation(
                        $"No hay horarios programados para {diaSemana} {manana:dd/MM/yyyy}"
                    );
                    return 0;
                }

                _logger.LogInformation(
                    $"Generando notificaciones para {horariosManana.Count} horarios del {diaSemana} {manana:dd/MM/yyyy}"
                );

                foreach (var horario in horariosManana)
                {
                    // Obtener usuario del docente
                    var usuario = await _context.Usuario
                        .FirstOrDefaultAsync(u => u.Id == horario.DocenteId);

                    if (usuario == null)
                    {
                        _logger.LogWarning(
                            $"No se encontró usuario para docente {horario.DocenteId} (horario {horario.Id})"
                        );
                        continue;
                    }

                    // Calcular fecha y hora exacta de la notificación (2 horas antes de la clase)
                    var fechaClase = manana.Add(horario.HoraInicio);
                    var fechaNotificacion = fechaClase.AddHours(-2);

                    // Verificar si ya existe notificación para este horario en esta fecha
                    var notificacionExiste = await _context.Notificacion
                        .AnyAsync(n =>
                            n.UsuarioId == usuario.Id &&
                            n.HorarioAcademicoId == horario.Id &&
                            n.Tipo == "PROXIMA_CLASE" &&
                            n.FechaProgramada.HasValue &&
                            n.FechaProgramada.Value.Date == fechaNotificacion.Date &&
                            n.Estado == true
                        );

                    if (notificacionExiste)
                    {
                        _logger.LogInformation(
                            $"Ya existe notificación PROXIMA_CLASE para horario {horario.Id} el {manana:dd/MM/yyyy}"
                        );
                        continue;
                    }

                    // Crear mensaje dinámico
                    string mensaje = $"Tienes clase de {horario.Materia.Nombre} " +
                                   $"en el Aula {horario.Aula.Codigo} " +
                                   $"en 2 horas ({horario.HoraInicio:HH\\:mm})";

                    // Crear notificación
                    var notificacion = new Notificacion
                    {
                        UsuarioId = usuario.Id,
                        Tipo = "PROXIMA_CLASE",
                        Mensaje = mensaje,
                        Prioridad = "MEDIA",
                        HorarioAcademicoId = horario.Id,
                        Leida = false,
                        FechaCreacion = DateTime.Now,
                        FechaProgramada = fechaNotificacion,
                        FechaEnvio = null, // Se enviará cuando llegue la hora
                        Estado = true
                    };

                    _context.Notificacion.Add(notificacion);
                    notificacionesCreadas++;

                    _logger.LogInformation(
                        $"Notificación PROXIMA_CLASE creada: {horario.Materia.Nombre} " +
                        $"a las {horario.HoraInicio:HH\\:mm} " +
                        $"(Notificación programada: {fechaNotificacion:dd/MM/yyyy HH:mm})"
                    );
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    $"✓ {notificacionesCreadas} notificaciones de próxima clase generadas para el {manana:dd/MM/yyyy}"
                );

                return notificacionesCreadas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar notificaciones de próxima clase");
                return 0;
            }
        }

        // ==========================================
        // LIMPIEZA DE NOTIFICACIONES ANTIGUAS
        // ==========================================

        /// <summary>
        /// Elimina notificaciones leídas con más de 30 días de antigüedad
        /// Ejecutar semanalmente para mantener la BD limpia
        /// </summary>
        public async Task<int> LimpiarNotificacionesAntiguasAsync()
        {
            try
            {
                var fechaLimite = DateTime.Now.AddDays(-30);

                var notificacionesAntiguasIds = await _context.Notificacion
                    .Where(n =>
                        n.Leida == true &&
                        n.FechaLectura.HasValue &&
                        n.FechaLectura.Value < fechaLimite
                    )
                    .Select(n => n.Id)
                    .ToListAsync();

                if (!notificacionesAntiguasIds.Any())
                {
                    _logger.LogInformation("No hay notificaciones antiguas para limpiar.");
                    return 0;
                }

                // Eliminar en lotes para mejor rendimiento
                foreach (var id in notificacionesAntiguasIds)
                {
                    var notificacion = await _context.Notificacion.FindAsync(id);
                    if (notificacion != null)
                    {
                        _context.Notificacion.Remove(notificacion);
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    $"✓ {notificacionesAntiguasIds.Count} notificaciones antiguas eliminadas " +
                    $"(leídas antes del {fechaLimite:dd/MM/yyyy})"
                );

                return notificacionesAntiguasIds.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar notificaciones antiguas");
                return 0;
            }
        }
    }
}