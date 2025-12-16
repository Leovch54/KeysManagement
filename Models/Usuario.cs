using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionLlaves.Models
{
    public class Usuario : BaseModel
    {
        [Key]
        [ForeignKey("Persona")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [MaxLength(150, ErrorMessage = "El correo no puede exceder 150 caracteres")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        [Display(Name = "Correo Electrónico")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatorio")]
        [Display(Name = "Contraseña de Acceso")]
        public byte[]? Contrasenia { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio")]
        [MaxLength(20, ErrorMessage = "El rol no puede exceder 20 caracteres")]
        [RegularExpression("^(DOCENTE|ADMIN)$",
            ErrorMessage = "El rol debe ser: DOCENTE o ADMIN")]
        [Display(Name = "Rol del Usuario")]
        public string? Rol { get; set; }

        [Display(Name = "Última Conexión")]
        [DataType(DataType.DateTime)]
        public DateTime? FechaUltimaConexion { get; set; }

        // Navegacion 
        public Persona? Persona { get; set; }
        public ICollection<Reserva>? Reservas { get; set; }

    }
}