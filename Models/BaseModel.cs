using System.ComponentModel.DataAnnotations;

namespace GestionLlaves.Models
{
    public abstract class BaseModel
    {
        [Display(Name = "Estado")]
        [Range(0, 1, ErrorMessage = "El estado debe ser 0 (inactivo) o 1 (activo)")]
        public bool Estado { get; set; } = true;

        [Display(Name = "Fecha de Creación")]
        [DataType(DataType.DateTime)]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El ID del usuario es obligatorio")]
        [Display(Name = "Usuario Responsable")]
        public int CreadoModPor { get; set; }

        [Display(Name = "Última Modificación")]
        [DataType(DataType.DateTime)]
        public DateTime? UltimaMod { get; set; }
    }
}