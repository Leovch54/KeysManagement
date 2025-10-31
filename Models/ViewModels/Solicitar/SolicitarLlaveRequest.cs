using System.ComponentModel.DataAnnotations;

namespace GestionLlaves.Models.ViewModels.Solicitar
{
    public class SolicitarLlaveRequest
    {
        /// <summary>
        /// ID de la reserva (solo si es tipo EXCEPCIONAL)
        /// </summary>
        public int? ReservaId { get; set; }

        /// <summary>
        /// ID del horario académico (solo si es tipo REGULAR)
        /// </summary>
        public int? HorarioAcademicoId { get; set; }

        /// <summary>
        /// ID del aula a solicitar (obligatorio)
        /// </summary>
        [Required(ErrorMessage = "El ID del aula es obligatorio")]
        public int AulaId { get; set; }

        /// <summary>
        /// Tipo de préstamo: "REGULAR" (clase) o "EXCEPCIONAL" (reserva)
        /// </summary>
        [Required(ErrorMessage = "El tipo de préstamo es obligatorio")]
        [RegularExpression("^(REGULAR|EXCEPCIONAL)$",
            ErrorMessage = "El tipo debe ser REGULAR o EXCEPCIONAL")]
        public string Tipo { get; set; } = "REGULAR";
    }
}
