using System;

namespace GestionLlaves.Models.ViewModels
{
    public class AdminSolicitudViewModel
    {
        public int Id { get; set; }
        public string TipoSolicitud { get; set; } // "PRESTAMO" o "RESERVA"
        public string DocenteNombre { get; set; }
        public string AulaCodigo { get; set; }
        public string EdificioNombre { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Estado { get; set; }
        // Para reservas: el propósito. Para préstamos: el tipo.
        public string Detalle { get; set; }
        public DateTime FechaSolicitud { get; set; }
    }
}