using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionLlaves.Models
{
    public class Aula : BaseModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El código del aula es obligatorio")]
        [MaxLength(20, ErrorMessage = "El código no puede exceder 20 caracteres")]
        [Display(Name = "Código del Aula")]
        public string? Codigo { get; set; }

        [Required(ErrorMessage = "El edificio es obligatorio")]
        [Display(Name = "Edificio")]
        [ForeignKey("Edificio")]
        public int EdificioId { get; set; }

        [Range(0, 20, ErrorMessage = "El piso debe estar entre 0 y 20")]
        [Display(Name = "Número de Piso")]
        public int? Piso { get; set; }

        [Range(1, 1000, ErrorMessage = "La capacidad debe estar entre 1 y 1000 personas")]
        [Display(Name = "Capacidad de Personas")]
        public int? Capacidad { get; set; }

        [Display(Name = "Tiene Proyector")]
        public bool TieneProyector { get; set; } = false;

        [Display(Name = "Tiene TV")]
        public bool TieneTv { get; set; } = false;

        [MaxLength(20, ErrorMessage = "El estado físico no puede exceder 20 caracteres")]
        [RegularExpression("^(DISPONIBLE|MANTENIMIENTO|FUERA_SERVICIO)$",
            ErrorMessage = "El estado físico debe ser: DISPONIBLE, MANTENIMIENTO o FUERA_SERVICIO")]
        [Display(Name = "Estado Físico")]
        public string? EstadoFisico { get; set; } = "DISPONIBLE";


        // Navegacion
        public Edificio? Edificio { get; set; }
        public ICollection<Prestamo>? Prestamos { get; set; }
        public ICollection<Reserva>? Reservas { get; set; }
        public ICollection<HorarioAcademico>? HorariosAcademicos { get; set; }
    }
}