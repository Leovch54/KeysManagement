using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionLlaves.Models
{
    public class Notificacion
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El usuario es obligatorio")]
        [Display(Name = "Usuario")]
        [ForeignKey("Usuario")]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "El tipo de notificación es obligatorio")]
        [MaxLength(50, ErrorMessage = "El tipo no puede exceder 50 caracteres")]
        [RegularExpression("^(RECORDATORIO_DEVOLUCION|SOLICITUD_APROBADA|SOLICITUD_RECHAZADA|PROXIMA_CLASE|LLAVE_NO_DEVUELTA|RESERVA_CANCELADA|CAMBIO_HORARIO)$",
            ErrorMessage = "El tipo debe ser: RECORDATORIO_DEVOLUCION, SOLICITUD_APROBADA, SOLICITUD_RECHAZADA, PROXIMA_CLASE, LLAVE_NO_DEVUELTA, RESERVA_CANCELADA o CAMBIO_HORARIO")]
        [Display(Name = "Tipo de Notificación")]
        public string? Tipo { get; set; }

        [Required(ErrorMessage = "El mensaje es obligatorio")]
        [Display(Name = "Mensaje")]
        public string? Mensaje { get; set; }

        [Required(ErrorMessage = "La prioridad es obligatoria")]
        [MaxLength(20, ErrorMessage = "La prioridad no puede exceder 20 caracteres")]
        [RegularExpression("^(ALTA|MEDIA|BAJA)$",
            ErrorMessage = "La prioridad debe ser: ALTA, MEDIA o BAJA")]
        [Display(Name = "Prioridad")]
        public string Prioridad { get; set; } = "MEDIA";

        [Display(Name = "Préstamo")]
        [ForeignKey("Prestamo")]
        public int? PrestamoId { get; set; }

        [Display(Name = "Reserva")]
        [ForeignKey("Reserva")]
        public int? ReservaId { get; set; }

        [Display(Name = "Horario Académico")]
        [ForeignKey("HorarioAcademico")]
        public int? HorarioAcademicoId { get; set; }

        [Display(Name = "Leída")]
        public bool Leida { get; set; } = false;

        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha de Lectura")]
        public DateTime? FechaLectura { get; set; }

        [Required(ErrorMessage = "La fecha de creación es obligatoria")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha Programada")]
        public DateTime? FechaProgramada { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha de Envío")]
        public DateTime? FechaEnvio { get; set; }

        [Display(Name = "Estado")]
        [Range(0, 1, ErrorMessage = "El estado debe ser 0 (inactivo) o 1 (activo)")]
        public bool Estado { get; set; } = true;

        // Navegación
        public Usuario? Usuario { get; set; }
        public Prestamo? Prestamo { get; set; }
        public Reserva? Reserva { get; set; }
        public HorarioAcademico? HorarioAcademico { get; set; }
    }
}