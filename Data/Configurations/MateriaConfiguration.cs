using GestionLlaves.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionLlaves.Data.Configurations
{
    public class MateriaConfiguration : IEntityTypeConfiguration<Materia>
    {
        public void Configure(EntityTypeBuilder<Materia> builder)
        {
            // Nombre de tabla
            builder.ToTable("materia");

            // Clave primaria
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            // Propiedades
            builder.Property(m => m.Nombre)
                   .HasColumnName("nombre")
                   .HasMaxLength(150)
                   .IsRequired();

            builder.Property(m => m.Estado)
                   .HasColumnName("estado")
                   .HasDefaultValue(true);

            builder.Property(m => m.FechaCreacion)
                   .HasColumnName("fechaCreacion")
                   .HasDefaultValueSql("GETDATE()");

            // Relaciones
            builder.HasMany(m => m.HorariosAcademicos)
                   .WithOne(h => h.Materia)
                   .HasForeignKey(h => h.MateriaId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}