using System.ComponentModel.DataAnnotations;

namespace GestionLlaves.Models
{
    public class Persona : BaseModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Los nombres son obligatorios")]
        [MaxLength(60, ErrorMessage = "Los nombres no pueden exceder 60 caracteres")]
        [Display(Name = "Nombres")]
        public string? Nombres { get; set; }

        [Required(ErrorMessage = "El primer apellido es obligatorio")]
        [MaxLength(60, ErrorMessage = "El primer apellido no puede exceder 60 caracteres")]
        [Display(Name = "Primer Apellido")]
        public string? PrimerApellido { get; set; }

        [MaxLength(60, ErrorMessage = "El segundo apellido no puede exceder 60 caracteres")]
        [Display(Name = "Segundo Apellido")]
        public string? SegundoApellido { get; set; }

        [MaxLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        [Phone(ErrorMessage = "El formato del teléfono no es válido")]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "El tipo de persona es obligatorio")]
        [MaxLength(20, ErrorMessage = "El tipo no puede exceder 20 caracteres")]
        [RegularExpression("^(DOCENTE|ESTUDIANTE|LIMPIEZA|ADMINISTRATIVO)$",
            ErrorMessage = "El tipo debe ser: DOCENTE, ESTUDIANTE, LIMPIEZA o ADMINISTRATIVO")]
        [Display(Name = "Tipo de Persona")]
        public string? Tipo { get; set; }

        // Navegacion
        public Usuario? Usuario { get; set; }
        public ICollection<Prestamo>? Prestamos { get; set; }
        public ICollection<HorarioAcademico>? HorariosAcademicos { get; set; }
    }
}