using NBomber.CSharp;
using NBomber.Http.CSharp;
using System.Net.Http.Json;
using System.Text;

namespace GestionLlaves.LoadTests;

public class Program
{
    public static void Main(string[] args)
    {
        // ============================================
        // ESCENARIO 1: CARGA DE LOGIN (Autenticación)
        // Requerimiento No Funcional: Tiempo de respuesta en alta demanda
        // ============================================
        var scenarioLogin = Scenario.Create("carga_login_autenticacion", async context =>
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:7081");

            var request = Http.CreateRequest("POST", "/Account/Login")
                .WithHeader("Content-Type", "application/x-www-form-urlencoded")
                .WithBody(new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("email", "test@univalle.edu.bo"),
                    new KeyValuePair<string, string>("contrasenia", "TestPassword123")
                }));

            var response = await Http.Send(request, context);

            return response;
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(5))
        .WithLoadSimulations(
            // Simular 20 docentes intentando iniciar sesión simultáneamente
            Simulation.InjectPerSec(rate: 20, during: TimeSpan.FromSeconds(60))
        );

        // ============================================
        // ESCENARIO 2: SOLICITUDES SIMULTÁNEAS DE LLAVES
        // Requerimiento Funcional: Gestión de solicitudes
        // Requerimiento No Funcional: Escalabilidad
        // ============================================
        var scenarioSolicitarLlave = Scenario.Create("carga_solicitudes_simultaneas", async context =>
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:7081");

            // Simular solicitud de llave por parte de un docente
            var requestBody = new
            {
                AulaId = 1,
                Tipo = "REGULAR",
                HorarioAcademicoId = (int?)null
            };

            var request = Http.CreateRequest("POST", "/Docente/SolicitarLlave")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("Cookie", "session=test-session-id")
                .WithBody(new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"));

            var response = await Http.Send(request, context);

            return response;
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(5))
        .WithLoadSimulations(
            // Simular 15 docentes solicitando llaves al mismo tiempo (escenario de cambio de clase)
            Simulation.InjectPerSec(rate: 15, during: TimeSpan.FromSeconds(60))
        );

        // ============================================
        // ESCENARIO 3: CONSULTA DE ESTADO (Dashboard Docente)
        // Requerimiento Funcional: Dashboard docente
        // ============================================
        var scenarioEstado = Scenario.Create("carga_consulta_estado", async context =>
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:7081");

            var request = Http.CreateRequest("GET", "/Docente/Estado")
                .WithHeader("Cookie", "session=test-session-id");

            var response = await Http.Send(request, context);

            return response;
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(5))
        .WithLoadSimulations(
            // Simular múltiples docentes consultando su estado
            Simulation.InjectPerSec(rate: 10, during: TimeSpan.FromSeconds(60))
        );

        // ============================================
        // ESCENARIO 4: ADMINISTRADOR MARCANDO DEVOLUCIONES
        // Requerimiento Funcional: Control de llaves
        // ============================================
        var scenarioMarcarDevuelto = Scenario.Create("carga_marcar_devuelto", async context =>
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:7081");

            var request = Http.CreateRequest("POST", "/Admin/MarcarDevuelto")
                .WithHeader("Content-Type", "application/x-www-form-urlencoded")
                .WithHeader("Cookie", "session=admin-session-id")
                .WithBody(new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("id", "1")
                }));

            var response = await Http.Send(request, context);

            return response;
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(5))
        .WithLoadSimulations(
            // Simular administrador procesando devoluciones
            Simulation.InjectPerSec(rate: 5, during: TimeSpan.FromSeconds(60))
        );

        // ============================================
        // ESCENARIO 5: PRUEBA DE STRESS - PICO DE DEMANDA
        // Requerimiento No Funcional: Escalabilidad y tiempo de respuesta
        // Simula el escenario de cambio de clase (máxima demanda)
        // ============================================
        var scenarioStress = Scenario.Create("stress_pico_demanda", async context =>
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:7081");

            // Simular múltiples operaciones simultáneas durante cambio de clase
            var random = new Random();
            var operaciones = new[]
            {
                "/Docente/Solicitar",
                "/Docente/Estado",
                "/Docente/Horario"
            };

            var operacion = operaciones[random.Next(operaciones.Length)];
            var request = Http.CreateRequest("GET", operacion)
                .WithHeader("Cookie", "session=test-session-id");

            var response = await Http.Send(request, context);

            return response;
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(10))
        .WithLoadSimulations(
            // Prueba de stress: 50 solicitudes por segundo durante 2 minutos
            // Simula el escenario real de cambio de clase con muchos docentes
            Simulation.InjectPerSec(rate: 50, during: TimeSpan.FromSeconds(120))
        );

        // ============================================
        // ESCENARIO 6: GENERACIÓN DE REPORTES
        // Requerimiento Funcional: Reportes
        // Requerimiento No Funcional: Rendimiento en operaciones pesadas
        // ============================================
        var scenarioReportes = Scenario.Create("carga_generacion_reportes", async context =>
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:7081");

            var desde = DateTime.Today.AddDays(-7).ToString("yyyy-MM-dd");
            var hasta = DateTime.Today.ToString("yyyy-MM-dd");

            var request = Http.CreateRequest("GET", $"/Admin/ReporteLlaves?desde={desde}&hasta={hasta}")
                .WithHeader("Cookie", "session=admin-session-id");

            var response = await Http.Send(request, context);

            return response;
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(5))
        .WithLoadSimulations(
            // Simular administrador generando reportes
            Simulation.InjectPerSec(rate: 2, during: TimeSpan.FromSeconds(30))
        );

        // Ejecutar todos los escenarios
        NBomberRunner
            .RegisterScenarios(
                scenarioLogin,
                scenarioSolicitarLlave,
                scenarioEstado,
                scenarioMarcarDevuelto,
                scenarioStress,
                scenarioReportes
            )
            .Run();
    }
}

