using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionLlaves.Models
{
    public class Reserva : BaseModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El solicitante es obligatorio")]
        [Display(Name = "Solicitante")]
        [ForeignKey("Solicitante")]
        public int SolicitanteId { get; set; }

        [Required(ErrorMessage = "El aula es obligatoria")]
        [Display(Name = "Aula")]
        [ForeignKey("Aula")]
        public int AulaId { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha y Hora de Inicio")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha y Hora de Fin")]
        public DateTime FechaFin { get; set; }

        [Required(ErrorMessage = "El propósito de la reserva es obligatorio")]
        [MaxLength(100, ErrorMessage = "El propósito no puede exceder 100 caracteres")]
        [Display(Name = "Propósito de la Reserva")]
        public string? Proposito { get; set; }

        [Required(ErrorMessage = "La justificación es obligatoria")]
        [MinLength(20, ErrorMessage = "La justificación debe tener al menos 20 caracteres")]
        [MaxLength(2000, ErrorMessage = "La justificación no puede exceder 2000 caracteres")]
        [Display(Name = "Justificación Detallada")]
        public string? Justificacion { get; set; }

        [MaxLength(15, ErrorMessage = "El estado no puede exceder 15 caracteres")]
        [RegularExpression("^(PENDIENTE|APROBADA|RECHAZADA|CANCELADA)$",
            ErrorMessage = "El estado debe ser: PENDIENTE, APROBADA, RECHAZADA o CANCELADA")]
        [Display(Name = "Estado de la Reserva")]
        public string? EstadoReserva { get; set; } = "PENDIENTE";

        [Display(Name = "Aprobada Por")]
        [ForeignKey("AprobadaPorUsuario")]
        public int? AprobadaPor { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha de Aprobación")]
        public DateTime? FechaAprobacion { get; set; }

        [MaxLength(500, ErrorMessage = "El motivo de rechazo no puede exceder 500 caracteres")]
        [Display(Name = "Motivo del Rechazo")]
        public string? MotivoRechazo { get; set; }

        // Navegacion
        public Usuario? Solicitante { get; set; }
        public Aula? Aula { get; set; }
        public Usuario? AprobadaPorUsuario { get; set; }
        public ICollection<Prestamo>? Prestamos { get; set; }
    }
}