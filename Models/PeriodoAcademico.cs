using System.ComponentModel.DataAnnotations;

namespace GestionLlaves.Models
{
    public class PeriodoAcademico : BaseModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del periodo académico es obligatorio")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre del Periodo")]
        public string? Nombre { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Inicio")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Fin")]
        public DateTime FechaFin { get; set; }

        [Range(0, 1, ErrorMessage = "El campo activo debe ser 0 (inactivo) o 1 (activo)")]
        [Display(Name = "Periodo Activo")]
        public bool Activo { get; set; } = false;

        // Navegacion 
        public ICollection<HorarioAcademico>? HorariosAcademicos { get; set; }
    }
}