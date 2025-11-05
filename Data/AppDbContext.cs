    using GestionLlaves.Models;
    using Microsoft.EntityFrameworkCore;

    namespace GestionLlaves.Data
    {
        public class AppDbContext : DbContext
        {
            public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
            {
            }

            public DbSet<Persona> Persona { get; set; }

            public DbSet<Usuario> Usuario { get; set; }

            public DbSet<Edificio> Edificio { get; set; }

            public DbSet<Aula> Aula { get; set; }

            public DbSet<PeriodoAcademico> PeriodoAcademico { get; set; }

            public DbSet<Materia> Materia { get; set; }

            public DbSet<HorarioAcademico> HorarioAcademico { get; set; }

            public DbSet<Reserva> Reserva { get; set; }

            public DbSet<Prestamo> Prestamo { get; set; }

            public DbSet<Notificacion> Notificacion { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
                }
            }
    }