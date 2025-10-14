using GestionLlaves.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionLlaves.Data.Configurations
{
    public class PeriodoAcademicoConfiguration : IEntityTypeConfiguration<PeriodoAcademico>
    {
        public void Configure(EntityTypeBuilder<PeriodoAcademico> builder)
        {
            // Nombre de tabla
            builder.ToTable("periodoAcademico");

            // Clave primaria
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            // Propiedades
            builder.Property(p => p.Nombre)
                   .HasColumnName("nombre")
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(p => p.FechaInicio)
                   .HasColumnName("fechaInicio")
                   .HasColumnType("DATE")
                   .IsRequired();

            builder.Property(p => p.FechaFin)
                   .HasColumnName("fechaFin")
                   .HasColumnType("DATE")
                   .IsRequired();

            builder.Property(p => p.Activo)
                   .HasColumnName("activo")
                   .HasDefaultValue(false);

            builder.Property(p => p.Estado)
                   .HasColumnName("estado")
                   .HasDefaultValue(true);

            builder.Property(p => p.FechaCreacion)
                   .HasColumnName("fechaCreacion")
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(a => a.CreadoModPor)
                   .HasColumnName("creadoModPor")
                   .IsRequired();

            builder.Property(p => p.UltimaMod)
                   .HasColumnName("ultimaMod")
                   .IsRequired(false);

            // Relaciones
            builder.HasMany(p => p.HorariosAcademicos)
                   .WithOne(h => h.PeriodoAcademico)
                   .HasForeignKey(h => h.PeriodoAcademicoId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}