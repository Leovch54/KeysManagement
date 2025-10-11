using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionLlaves.Models
{
    public class HorarioAcademico : BaseModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El periodo académico es obligatorio")]
        [Display(Name = "Periodo Académico")]
        [ForeignKey("PeriodoAcademico")]
        public int PeriodoAcademicoId { get; set; }

        [Required(ErrorMessage = "La materia es obligatoria")]
        [Display(Name = "Materia")]
        [ForeignKey("Materia")]
        public int MateriaId { get; set; }

        [Required(ErrorMessage = "El docente es obligatorio")]
        [Display(Name = "Docente")]
        [ForeignKey("Docente")]
        public int DocenteId { get; set; }

        [Required(ErrorMessage = "El aula es obligatoria")]
        [Display(Name = "Aula")]
        [ForeignKey("Aula")]
        public int AulaId { get; set; }

        [Required(ErrorMessage = "El grupo es obligatorio")]
        [MaxLength(5, ErrorMessage = "El grupo no puede exceder 5 caracteres")]
        [Display(Name = "Grupo")]
        public string? Grupo { get; set; }

        [Required(ErrorMessage = "La hora de inicio es obligatoria")]
        [DataType(DataType.Time)]
        [Display(Name = "Hora de Inicio")]
        public TimeSpan HoraInicio { get; set; }

        [Required(ErrorMessage = "La hora de fin es obligatoria")]
        [DataType(DataType.Time)]
        [Display(Name = "Hora de Fin")]
        public TimeSpan HoraFin { get; set; }

        // Elimino campo "Dia" y agregar campos BIT
        [Display(Name = "Lunes")]
        public bool Lunes { get; set; } = false;

        [Display(Name = "Martes")]
        public bool Martes { get; set; } = false;

        [Display(Name = "Miércoles")]
        public bool Miercoles { get; set; } = false;

        [Display(Name = "Jueves")]
        public bool Jueves { get; set; } = false;

        [Display(Name = "Viernes")]
        public bool Viernes { get; set; } = false;

        [Display(Name = "Sábado")]
        public bool Sabado { get; set; } = false;
        // ========================================

        [Display(Name = "Estado del Horario")]
        public bool EstadoHorario { get; set; } = true;

        // Navegacion
        public PeriodoAcademico? PeriodoAcademico { get; set; }
        public Materia? Materia { get; set; }
        public Persona? Docente { get; set; }
        public Aula? Aula { get; set; }
        public ICollection<Prestamo>? Prestamos { get; set; }
    }
}