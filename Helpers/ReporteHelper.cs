using System.Text.Json;
using GestionLlaves.Models;

namespace GestionLlaves.Helpers
{
    public static class ReporteHelper
    {
        private static readonly string FilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "reportes_extras.json");

        public static List<Reportes> CargarReportes()
        {
            if (!File.Exists(FilePath)) return new List<Reportes>();
            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<Reportes>>(json) ?? new List<Reportes>();
        }

        public static void GuardarReportes(List<Reportes> lista)
        {
            var json = JsonSerializer.Serialize(lista, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
    }
}
