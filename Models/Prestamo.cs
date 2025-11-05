using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionLlaves.Models
{
    public class Prestamo : BaseModel
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Reserva")]
        public int? ReservaId { get; set; }

        [Display(Name = "Horario Académico")]
        public int? HorarioAcademicoId { get; set; }

        [Required(ErrorMessage = "La persona es obligatoria")]
        [Display(Name = "Persona")]
        public int PersonaId { get; set; }

        [Required(ErrorMessage = "El aula es obligatoria")]
        [Display(Name = "Aula")]
        public int AulaId { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha y Hora de Inicio")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha fin programada es obligatoria")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha y Hora Fin Programada")]
        public DateTime FechaFinProgramada { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha y Hora de Devolución Real")]
        public DateTime? FechaFinReal { get; set; }

        [Required(ErrorMessage = "El tipo de préstamo es obligatorio")]
        [MaxLength(15, ErrorMessage = "El tipo no puede exceder 15 caracteres")]
        [RegularExpression("^(REGULAR|EXCEPCIONAL|MANUAL)$",
            ErrorMessage = "El tipo debe ser: REGULAR, EXCEPCIONAL o MANUAL")]
        [Display(Name = "Tipo de Préstamo")]
        public string? Tipo { get; set; }

        [MaxLength(15, ErrorMessage = "El estado no puede exceder 15 caracteres")]
        [RegularExpression("^(ACTIVO|DEVUELTO|VENCIDO)$",
            ErrorMessage = "El estado debe ser: ACTIVO, DEVUELTO o VENCIDO")]
        [Display(Name = "Estado del Préstamo")]
        public string? EstadoPrestamo { get; set; } = "ACTIVO";

        [Display(Name = "Recibido Por")]
        [ForeignKey("RecibidoPorUsuario")]
        public int? RecibidoPor { get; set; }

        // Navegacion
        public Persona? Persona { get; set; }
        public Aula? Aula { get; set; }
        public Reserva? Reserva { get; set; }
        public HorarioAcademico? HorarioAcademico { get; set; }
        public Usuario? RecibidoPorUsuario { get; set; }
    }
}