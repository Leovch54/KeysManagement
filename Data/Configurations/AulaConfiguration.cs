using GestionLlaves.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionLlaves.Data.Configurations
{
    public class AulaConfiguration : IEntityTypeConfiguration<Aula>
    {
        public void Configure(EntityTypeBuilder<Aula> builder)
        {
            // Nombre de tabla
            builder.ToTable("aula");

            // Clave primaria
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            // Propiedades
            builder.Property(a => a.Codigo)
                   .HasColumnName("codigo")
                   .HasMaxLength(20)
                   .IsRequired();

            builder.HasIndex(a => a.Codigo)
                   .IsUnique();

            builder.Property(a => a.EdificioId)
                   .HasColumnName("edificioId")
                   .IsRequired();

            builder.Property(a => a.Piso)
                   .HasColumnName("piso")
                   .IsRequired(false);

            builder.Property(a => a.Capacidad)
                   .HasColumnName("capacidad")
                   .IsRequired(false);

            builder.Property(a => a.TieneProyector)
                   .HasColumnName("tieneProyector")
                   .HasDefaultValue(false);

            builder.Property(a => a.TieneTv)
                   .HasColumnName("tieneTv")
                   .HasDefaultValue(false);

            builder.Property(a => a.EstadoFisico)
                   .HasColumnName("estadoFisico")
                   .HasMaxLength(20)
                   .HasDefaultValue("DISPONIBLE");

            builder.Property(a => a.Estado)
                   .HasColumnName("estado")
                   .HasDefaultValue(true);

            builder.Property(a => a.FechaCreacion)
                   .HasColumnName("fechaCreacion")
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(a => a.CreadoModPor)
                   .HasColumnName("creadoModPor")
                   .IsRequired();

            builder.Property(a => a.UltimaMod)
                   .HasColumnName("ultimaMod")
                   .IsRequired(false);

            // Relaciones
            builder.HasOne(a => a.Edificio)
                   .WithMany(e => e.Aulas)
                   .HasForeignKey(a => a.EdificioId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(a => a.HorariosAcademicos)
                   .WithOne(h => h.Aula)
                   .HasForeignKey(h => h.AulaId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(a => a.Reservas)
                   .WithOne(r => r.Aula)
                   .HasForeignKey(r => r.AulaId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(a => a.Prestamos)
                   .WithOne(p => p.Aula)
                   .HasForeignKey(p => p.AulaId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}