namespace GestionLlaves.Models.ViewModels.Solicitar
{
    public class ItemSolicitarViewModel
    {
        public string Tipo { get; set; } = string.Empty; // "CLASE" o "RESERVA"

        public int? ReservaId { get; set; }

        public int? HorarioAcademicoId { get; set; }

        public int AulaId { get; set; }

        public string Titulo { get; set; } = string.Empty;

        public string Subtitulo { get; set; } = string.Empty;

        public DateTime HoraInicio { get; set; }

        public DateTime HoraFin { get; set; }

        public string TipoPrestamo { get; set; } = "REGULAR"; // REGULAR o EXCEPCIONAL

        public TimeSpan TiempoRestante => HoraFin - DateTime.Now;

        public string TiempoRestanteFormateado
        {
            get
            {
                var tiempo = TiempoRestante;
                if (tiempo.TotalHours >= 1)
                    return $"{(int)tiempo.TotalHours}h {tiempo.Minutes} min";
                return $"{tiempo.Minutes} min";
            }
        }

        public string HorarioFormateado => $"{HoraInicio:hh:mm tt} - {HoraFin:hh:mm tt}";
    }
}