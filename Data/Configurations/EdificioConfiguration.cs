using GestionLlaves.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionLlaves.Data.Configurations
{
    public class EdificioConfiguration : IEntityTypeConfiguration<Edificio>
    {
        public void Configure(EntityTypeBuilder<Edificio> builder)
        {
            // Nombre de tabla
            builder.ToTable("edificio");

            // Clave primaria
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            // Propiedades
            builder.Property(e => e.Nombre)
                   .HasColumnName("nombre")
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(e => e.Codigo)
                   .HasColumnName("codigo")
                   .HasMaxLength(10)
                   .IsRequired();

            builder.HasIndex(e => e.Codigo)
                   .IsUnique();

            builder.Property(e => e.Direccion)
                   .HasColumnName("direccion")
                   .HasMaxLength(200)
                   .IsRequired(false);

            builder.Property(e => e.Estado)
                   .HasColumnName("estado")
                   .HasDefaultValue(true);

            builder.Property(e => e.FechaCreacion)
                   .HasColumnName("fechaCreacion")
                   .HasDefaultValueSql("GETDATE()");

            // Relaciones
            builder.HasMany(e => e.Aulas)
                   .WithOne(a => a.Edificio)
                   .HasForeignKey(a => a.EdificioId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}