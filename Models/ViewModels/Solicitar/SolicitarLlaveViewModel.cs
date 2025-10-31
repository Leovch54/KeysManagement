using System.ComponentModel.DataAnnotations;

namespace GestionLlaves.Models.ViewModels.Solicitar
{
    public class SolicitarLlaveViewModel
    {
        public string NombreDocente { get; set; } = string.Empty;
        public DateTime FechaActual { get; set; } = DateTime.Now;

        public ItemSolicitarViewModel? ItemSolicitar { get; set; }

        public List<LlaveActivaViewModel> LlavesActivas { get; set; } = new();

        public List<ProximaClaseViewModel> ProximasClases { get; set; } = new();
    }

}