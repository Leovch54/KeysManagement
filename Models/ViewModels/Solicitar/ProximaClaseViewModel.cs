namespace GestionLlaves.Models.ViewModels.Solicitar
{
    public class ProximaClaseViewModel
    {
        /// <summary>
        /// ID del horario académico
        /// </summary>
        public int HorarioAcademicoId { get; set; }

        /// <summary>
        /// Nombre de la materia
        /// </summary>
        public string NombreMateria { get; set; } = string.Empty;

        /// <summary>
        /// Código del aula (ej: "A-101")
        /// </summary>
        public string CodigoAula { get; set; } = string.Empty;

        /// <summary>
        /// Hora de inicio de la clase (TimeSpan)
        /// </summary>
        public TimeSpan HoraInicio { get; set; }

        /// <summary>
        /// Hora de fin de la clase (TimeSpan)
        /// </summary>
        public TimeSpan HoraFin { get; set; }

        /// <summary>
        /// Fecha de la clase (sin hora)
        /// </summary>
        public DateTime FechaClase { get; set; }

        /// <summary>
        /// Indica si la clase es hoy
        /// </summary>
        public bool EsHoy => FechaClase.Date == DateTime.Today;

        /// <summary>
        /// Texto del día: "Hoy" o "Mañana"
        /// </summary>
        public string DiaTexto => EsHoy ? "Hoy" : "Mañana";

        /// <summary>
        /// Formatea el horario en formato 24h (ej: "08:00 - 10:00")
        /// </summary>
        public string HorarioFormateado => $"{HoraInicio:hh\\:mm} - {HoraFin:hh\\:mm}";
    }
}
