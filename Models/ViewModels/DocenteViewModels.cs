namespace GestionLlaves.Models.ViewModels
{
    public class HorarioItemVM
    {
        public int HorarioId { get; set; }
        public string Materia { get; set; } = "";
        public string Aula { get; set; } = "";
        public TimeSpan Inicio { get; set; }
        public TimeSpan Fin { get; set; }
        public bool EnCurso { get; set; }
    }

    public class EstadoVM
    {
        public bool TienePrestamoActivo { get; set; }
        public string? AulaActual { get; set; }
        public DateTime? FinProgramado { get; set; }
        public TimeSpan? TiempoRestante { get; set; }

        public int TotalPrestamos { get; set; }
        public int DevolucionesATiempo { get; set; }
        public double PorcentajeATiempo => TotalPrestamos == 0 ? 0 : (double)DevolucionesATiempo / TotalPrestamos * 100.0;
        public int RachaATiempo { get; set; }

        public List<HistorialItem> Historial { get; set; } = new();
        public class HistorialItem
        {
            public DateTime Fecha { get; set; }
            public string Aula { get; set; } = "";
            public bool ATiempo { get; set; }
        }
    }

    public class ReservaNuevaVM
    {
        public DateOnly Fecha { get; set; } = DateOnly.FromDateTime(DateTime.Today);
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public int? AulaId { get; set; }
        public string Proposito { get; set; } = "";
        public string Justificacion { get; set; } = "";
        public List<AulaOpcion> AulasDisponibles { get; set; } = new();
        public class AulaOpcion
        {
            public int Id { get; set; }
            public string Nombre { get; set; } = "";
            public string Edificio { get; set; } = "";
            public int Capacidad { get; set; }
        }
    }

    public class ReservaCardVM
    {
        public int Id { get; set; }
        public string Aula { get; set; } = "";
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }
        public string Estado { get; set; } = "";
        public string EtiquetaTiempo => Inicio.Date == DateTime.Today ? "Hoy" :
                                        Inicio.Date == DateTime.Today.AddDays(1) ? "Mañana" :
                                        Inicio.ToString("dd MMM");
    }
}
