namespace GestionLlaves.Models.ViewModels.Solicitar
{
    public class LlaveActivaViewModel
    {
        /// <summary>
        /// ID del préstamo
        /// </summary>
        public int PrestamoId { get; set; }

        /// <summary>
        /// Código del aula (ej: "A-101")
        /// </summary>
        public string CodigoAula { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del edificio (ej: "Edificio Central")
        /// </summary>
        public string NombreEdificio { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del préstamo (Nombre de la materia o Propósito de la reserva)
        /// </summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Fecha y hora de inicio del préstamo
        /// </summary>
        public DateTime HoraInicio { get; set; }

        /// <summary>
        /// Fecha y hora de fin programada del préstamo
        /// </summary>
        public DateTime HoraFin { get; set; }

        /// <summary>
        /// Estado actual del préstamo (normalmente "ACTIVO")
        /// </summary>
        public string EstadoPrestamo { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el préstamo está en uso actualmente (true) o es próximo (false)
        /// </summary>
        public bool EsEnUso { get; set; }

        /// <summary>
        /// Calcula el tiempo restante:
        /// - Si está en uso: tiempo hasta que termine (HoraFin - ahora)
        /// - Si es próximo: tiempo hasta que comience (HoraInicio - ahora)
        /// </summary>
        public TimeSpan TiempoRestante => EsEnUso
            ? HoraFin - DateTime.Now
            : HoraInicio - DateTime.Now;

        /// <summary>
        /// Formatea el tiempo restante en formato legible (ej: "2h 30min")
        /// </summary>
        public string TiempoRestanteFormateado
        {
            get
            {
                var tiempo = TiempoRestante;
                if (tiempo.TotalMinutes < 0)
                    return "Vencido";
                if (tiempo.TotalHours >= 1)
                    return $"{(int)tiempo.TotalHours}h {tiempo.Minutes}min";
                return $"{tiempo.Minutes}min";
            }
        }

        /// <summary>
        /// Texto del estado: "En uso" o "Próximo"
        /// </summary>
        public string EstadoTexto => EsEnUso ? "En uso" : "Próximo";

        /// <summary>
        /// Clase CSS para el estado: "en-uso" o "proximo"
        /// </summary>
        public string ClaseEstado => EsEnUso ? "en-uso" : "proximo";

        /// <summary>
        /// Formatea el horario completo en formato 12h (ej: "08:00 AM - 10:00 AM")
        /// </summary>
        public string HorarioFormateado => $"{HoraInicio:hh:mm tt} - {HoraFin:hh:mm tt}";
    }
}
