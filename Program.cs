using Microsoft.EntityFrameworkCore;
using Sistema_Gestion_Llaves.Models; // 👈 tu DbContext generado por Scaffold

namespace Sistema_Gestion_Llaves
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 🔹 1. Configurar la conexión a SQL Server
            builder.Services.AddDbContext<SistemaGestionLlavesContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 🔹 2. Agregar controladores con vistas
            builder.Services.AddControllersWithViews();

            // 🔹 3. Configurar sesiones
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // tiempo de sesión
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            // 🔹 4. Configuración de la tubería de ejecución
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            // 🔹 5. Activar sesiones
            app.UseSession();

            // 🔹 6. Activar autorización (con sesiones manuales ya es suficiente)
            app.UseAuthorization();

            // 🔹 7. Rutas predeterminadas
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
