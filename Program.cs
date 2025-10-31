using GestionLlaves.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------
// Configurar conexión a la base de datos
// -----------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// -----------------------------------------
// Agregar servicios MVC
// -----------------------------------------
builder.Services.AddControllersWithViews();

// -----------------------------------------
// Configurar sesiones
// -----------------------------------------
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2); // duración de sesión
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

    // ⚠️ En localhost, usar None para evitar conflictos de HTTPS
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
});

var app = builder.Build();

// -----------------------------------------
// Configuración del pipeline HTTP
// -----------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ IMPORTANTE: activar sesión antes de autorización
app.UseSession();

app.UseAuthorization();

// -----------------------------------------
// Ruta por defecto
// -----------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// -----------------------------------------
app.Run();
