using GestionLlaves.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GestionLlaves.Services
{
    /// <summary>
    /// Servicio de fondo que ejecuta tareas de notificaciones automáticas periódicamente
    /// </summary>
    public class NotificacionBackgroundService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificacionBackgroundService> _logger;

        private Timer? _timerEnvioProgramadas;
        private Timer? _timerMonitoreoVencidas;
        private Timer? _timerGeneracionProximaClase;
        private Timer? _timerLimpieza;

        // Configuración de intervalos de ejecución
        private readonly TimeSpan _intervaloEnvioProgramadas = TimeSpan.FromMinutes(5);      // Cada 5 minutos
        private readonly TimeSpan _intervaloMonitoreoVencidas = TimeSpan.FromMinutes(10);    // Cada 10 minutos
        private readonly TimeSpan _intervaloGeneracionClases = TimeSpan.FromHours(24);       // Cada 24 horas
        private readonly TimeSpan _intervaloLimpieza = TimeSpan.FromDays(7);                 // Cada 7 días

        public NotificacionBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<NotificacionBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        // ==========================================
        // INICIO DEL SERVICIO
        // ==========================================

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("═══════════════════════════════════════════════════════════");
            _logger.LogInformation("  NOTIFICACIÓN BACKGROUND SERVICE INICIADO");
            _logger.LogInformation("═══════════════════════════════════════════════════════════");
            _logger.LogInformation($"Envío programadas: cada {_intervaloEnvioProgramadas.TotalMinutes} min");
            _logger.LogInformation($"Monitoreo vencidas: cada {_intervaloMonitoreoVencidas.TotalMinutes} min");
            _logger.LogInformation($"Generación próxima clase: cada {_intervaloGeneracionClases.TotalHours} h");
            _logger.LogInformation($"Limpieza: cada {_intervaloLimpieza.TotalDays} días");
            _logger.LogInformation("═══════════════════════════════════════════════════════════");

            // Timer 1: Envío de notificaciones programadas
            _timerEnvioProgramadas = new Timer(
                EnviarNotificacionesProgramadas,
                null,
                TimeSpan.Zero,                   // Ejecutar inmediatamente al iniciar
                _intervaloEnvioProgramadas       // Luego cada 5 minutos
            );

            // Timer 2: Monitoreo de llaves no devueltas
            _timerMonitoreoVencidas = new Timer(
                MonitorearLlavesNoDevueltas,
                null,
                TimeSpan.FromMinutes(1),         // Esperar 1 minuto antes del primer check
                _intervaloMonitoreoVencidas      // Luego cada 10 minutos
            );

            // Timer 3: Generación de notificaciones de próxima clase
            // Se ejecuta a medianoche (00:00)
            var ahoraLocal = DateTime.Now;
            var proximaMedianoche = ahoraLocal.Date.AddDays(1); // Mañana a las 00:00
            var tiempoHastaMedianoche = proximaMedianoche - ahoraLocal;

            _timerGeneracionProximaClase = new Timer(
                GenerarNotificacionesProximaClase,
                null,
                tiempoHastaMedianoche,           // Primera ejecución a medianoche
                _intervaloGeneracionClases       // Luego cada 24 horas
            );

            _logger.LogInformation(
                $"Generación próxima clase programada para: {proximaMedianoche:dd/MM/yyyy HH:mm:ss}"
            );

            // Timer 4: Limpieza de notificaciones antiguas
            // Se ejecuta los domingos a las 03:00 AM
            var proximoDomingo = CalcularProximoDomingo3AM();
            var tiempoHastaDomingo = proximoDomingo - ahoraLocal;

            _timerLimpieza = new Timer(
                LimpiarNotificacionesAntiguas,
                null,
                tiempoHastaDomingo,              // Primera ejecución el próximo domingo
                _intervaloLimpieza               // Luego cada 7 días
            );

            _logger.LogInformation(
                $"Limpieza de notificaciones programada para: {proximoDomingo:dd/MM/yyyy HH:mm:ss}"
            );

            return Task.CompletedTask;
        }

        // ==========================================
        // TAREA 1: ENVÍO DE NOTIFICACIONES PROGRAMADAS
        // ==========================================

        private async void EnviarNotificacionesProgramadas(object? state)
        {
            try
            {
                _logger.LogInformation("───────────────────────────────────────────────────────────");
                _logger.LogInformation($"[{DateTime.Now:HH:mm:ss}] Iniciando envío de notificaciones programadas...");

                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<NotificacionService>>();

                var notificacionService = new NotificacionService(context, logger);
                var enviadas = await notificacionService.EnviarNotificacionesProgramadasAsync();

                if (enviadas > 0)
                {
                    _logger.LogInformation($"✓ {enviadas} notificaciones enviadas exitosamente");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico en EnviarNotificacionesProgramadas");
            }
        }

        // ==========================================
        // TAREA 2: MONITOREO DE LLAVES NO DEVUELTAS
        // ==========================================

        private async void MonitorearLlavesNoDevueltas(object? state)
        {
            try
            {
                _logger.LogInformation("───────────────────────────────────────────────────────────");
                _logger.LogInformation($"[{DateTime.Now:HH:mm:ss}] Iniciando monitoreo de llaves no devueltas...");

                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<NotificacionService>>();

                var notificacionService = new NotificacionService(context, logger);
                var creadas = await notificacionService.MonitorearLlavesNoDevueltasAsync();

                if (creadas > 0)
                {
                    _logger.LogWarning($"⚠️ {creadas} alertas de llaves no devueltas generadas");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico en MonitorearLlavesNoDevueltas");
            }
        }

        // ==========================================
        // TAREA 3: GENERACIÓN DE NOTIFICACIONES DE PRÓXIMA CLASE
        // ==========================================

        private async void GenerarNotificacionesProximaClase(object? state)
        {
            try
            {
                _logger.LogInformation("═══════════════════════════════════════════════════════════");
                _logger.LogInformation($"[{DateTime.Now:HH:mm:ss}] TAREA NOCTURNA: Generando notificaciones de próxima clase...");
                _logger.LogInformation("═══════════════════════════════════════════════════════════");

                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<NotificacionService>>();

                var notificacionService = new NotificacionService(context, logger);
                var creadas = await notificacionService.GenerarNotificacionesProximaClaseAsync();

                _logger.LogInformation($"✓ Tarea nocturna completada: {creadas} notificaciones generadas");
                _logger.LogInformation("═══════════════════════════════════════════════════════════");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico en GenerarNotificacionesProximaClase");
            }
        }

        // ==========================================
        // TAREA 4: LIMPIEZA DE NOTIFICACIONES ANTIGUAS
        // ==========================================

        private async void LimpiarNotificacionesAntiguas(object? state)
        {
            try
            {
                _logger.LogInformation("═══════════════════════════════════════════════════════════");
                _logger.LogInformation($"[{DateTime.Now:HH:mm:ss}] TAREA SEMANAL: Limpieza de notificaciones antiguas...");
                _logger.LogInformation("═══════════════════════════════════════════════════════════");

                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<NotificacionService>>();

                var notificacionService = new NotificacionService(context, logger);
                var eliminadas = await notificacionService.LimpiarNotificacionesAntiguasAsync();

                _logger.LogInformation($"✓ Tarea semanal completada: {eliminadas} notificaciones eliminadas");
                _logger.LogInformation("═══════════════════════════════════════════════════════════");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico en LimpiarNotificacionesAntiguas");
            }
        }

        // ==========================================
        // MÉTODOS AUXILIARES
        // ==========================================

        /// <summary>
        /// Calcula la fecha y hora del próximo domingo a las 03:00 AM
        /// </summary>
        private DateTime CalcularProximoDomingo3AM()
        {
            var ahora = DateTime.Now;
            var diasHastaDomingo = ((int)DayOfWeek.Sunday - (int)ahora.DayOfWeek + 7) % 7;

            // Si hoy es domingo y aún no son las 03:00, ejecutar hoy
            if (diasHastaDomingo == 0 && ahora.Hour < 3)
            {
                return ahora.Date.AddHours(3);
            }

            // Si no, calcular el próximo domingo
            if (diasHastaDomingo == 0)
            {
                diasHastaDomingo = 7;
            }

            return ahora.Date.AddDays(diasHastaDomingo).AddHours(3);
        }

        // ==========================================
        // DETENCIÓN DEL SERVICIO
        // ==========================================

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("═══════════════════════════════════════════════════════════");
            _logger.LogInformation("  NOTIFICACIÓN BACKGROUND SERVICE DETENIDO");
            _logger.LogInformation("═══════════════════════════════════════════════════════════");

            _timerEnvioProgramadas?.Change(Timeout.Infinite, 0);
            _timerMonitoreoVencidas?.Change(Timeout.Infinite, 0);
            _timerGeneracionProximaClase?.Change(Timeout.Infinite, 0);
            _timerLimpieza?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        // ==========================================
        // LIBERACIÓN DE RECURSOS
        // ==========================================

        public void Dispose()
        {
            _timerEnvioProgramadas?.Dispose();
            _timerMonitoreoVencidas?.Dispose();
            _timerGeneracionProximaClase?.Dispose();
            _timerLimpieza?.Dispose();
        }
    }
}