using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GestionLlaves.Models;

namespace GestionLlaves.Data.Configurations
{
    public class PersonaConfiguration : IEntityTypeConfiguration<Persona>
    {
        public void Configure(EntityTypeBuilder<Persona> builder)
        {
            // Nombre de tabla
            builder.ToTable("persona");

            // Clave primaria
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            // Propiedades
            builder.Property(p => p.Nombres)
                   .HasColumnName("nombres")
                   .HasMaxLength(60)
                   .IsRequired();

            builder.Property(p => p.PrimerApellido)
                   .HasColumnName("primerApellido")
                   .HasMaxLength(60)
                   .IsRequired();

            builder.Property(p => p.SegundoApellido)
                   .HasColumnName("segundoApellido")
                   .HasMaxLength(60)
                   .IsRequired(false);

            builder.Property(p => p.Telefono)
                   .HasColumnName("telefono")
                   .HasMaxLength(20)
                   .IsRequired(false);

            builder.Property(p => p.Tipo)
                   .HasColumnName("tipo")
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(p => p.Estado)
                   .HasColumnName("estado")
                   .HasDefaultValue(true);

            builder.Property(p => p.FechaCreacion)
                   .HasColumnName("fechaCreacion")
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(p => p.CreadoModPor)
                   .HasColumnName("creadoModPor")
                   .IsRequired();

            builder.Property(p => p.UltimaMod)
                   .HasColumnName("ultimaMod")
                   .IsRequired(false);

            // Relaciones
            builder.HasOne(p => p.Usuario)
                   .WithOne(u => u.Persona)
                   .HasForeignKey<Usuario>(u => u.Id)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.Prestamos)
                   .WithOne(pr => pr.Persona)
                   .HasForeignKey(pr => pr.PersonaId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.HorariosAcademicos)
                   .WithOne(h => h.Docente)
                   .HasForeignKey(h => h.DocenteId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
