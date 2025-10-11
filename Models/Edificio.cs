using System.ComponentModel.DataAnnotations;

namespace GestionLlaves.Models
{
    public class Edificio
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del edificio es obligatorio")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre del Edificio")]
        public string? Nombre { get; set; }

        [Required(ErrorMessage = "El código del edificio es obligatorio")]
        [MaxLength(10, ErrorMessage = "El código no puede exceder 10 caracteres")]
        [Display(Name = "Código del Edificio")]
        public string? Codigo { get; set; }

        [MaxLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [Display(Name = "Estado")]
        [Range(0, 1, ErrorMessage = "El estado debe ser 0 (inactivo) o 1 (activo)")]
        public bool Estado { get; set; } = true;

        [Display(Name = "Fecha de Creación")]
        [DataType(DataType.DateTime)]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Navegacion
        public ICollection<Aula>? Aulas { get; set; }
    }
}