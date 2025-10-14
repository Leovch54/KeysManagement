using System.ComponentModel.DataAnnotations;

namespace GestionLlaves.Models
{
    public class Materia
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la materia es obligatorio")]
        [MaxLength(150, ErrorMessage = "El nombre no puede exceder 150 caracteres")]
        [Display(Name = "Nombre de la Materia")]
        public string? Nombre { get; set; }

        [Display(Name = "Estado")]
        [Range(0, 1, ErrorMessage = "El estado debe ser 0 (inactivo) o 1 (activo)")]
        public bool Estado { get; set; } = true;

        [Display(Name = "Fecha de Creación")]
        [DataType(DataType.DateTime)]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Navegacion
        public ICollection<HorarioAcademico>? HorariosAcademicos { get; set; }

    }
}