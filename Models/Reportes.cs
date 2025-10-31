namespace GestionLlaves.Models
{
    public class Reportes
    {
        public int Id { get; set; }                    // ID del registro (vinculado a la reserva)
        public string Area { get; set; }               // Limpieza / Mantenimiento / Estudiante
        public string Nombre { get; set; }             // Nombre escrito en el campo
        public string Aula { get; set; }               // Código de aula (T-206, S-114, etc.)
        public DateTime FechaSolicitud { get; set; }   // Fecha y hora cuando se agregó
        public DateTime? FechaDevolucion { get; set; } // Fecha y hora cuando se marcó devuelto
        public string Estado { get; set; }
    }
}
