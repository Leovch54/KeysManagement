namespace GestionLlaves.Models.ViewModels.Solicitar
{
    public class ReservaAprobadaViewModel
    {
        public int ReservaId { get; set; }
        public string NombreAula { get; set; } = string.Empty;
        public string Proposito { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public TimeSpan TiempoRestante => FechaFin - DateTime.Now;
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
    }
}
