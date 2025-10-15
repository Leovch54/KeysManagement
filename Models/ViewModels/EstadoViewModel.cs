namespace GestionLlaves.Models.ViewModels
{
    public class EstadoViewModel
    {
        // Datos del préstamo activo
        public string AulaNombre { get; set; }
        public string MateriaNombre { get; set; }
        public DateTime HoraInicio { get; set; }
        public DateTime HoraFin { get; set; }
        public double PorcentajeTranscurrido { get; set; }
        public TimeSpan TiempoRestante { get; set; }

        // Estadísticas del mes
        public int TotalPrestamos { get; set; }
        public int DevolucionesATiempo { get; set; }
        public double PorcentajeATiempo => TotalPrestamos > 0 ? ((double)DevolucionesATiempo / TotalPrestamos) * 100 : 0;
        public TimeSpan PromedioUso { get; set; }
        public int PrestamosSinRetraso { get; set; }
        public TimeSpan RetrasoPromedio { get; set; }
        public int RachaDias { get; set; }
    }
}
