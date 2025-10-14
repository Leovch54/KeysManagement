using GestionLlaves.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionLlaves.Data.Configurations
{
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            // Nombre de tabla
            builder.ToTable("usuario");

            // Clave primaria (compartida con Persona)
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id)
                   .HasColumnName("id")
                   .ValueGeneratedNever(); // No se autogenera, usa el Id de Persona

            // Propiedades
            builder.Property(u => u.Email)
                   .HasColumnName("email")
                   .HasMaxLength(150)
                   .IsRequired();

            builder.HasIndex(u => u.Email)
                   .IsUnique();

            builder.Property(u => u.Contrasenia)
                   .HasColumnName("contrasenia")
                   .HasColumnType("VARBINARY(64)")
                   .IsRequired();

            builder.Property(u => u.Rol)
                   .HasColumnName("rol")
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(u => u.FechaUltimaConexion)
                   .HasColumnName("fechaUltimaConexion")
                   .IsRequired(false);

            builder.Property(u => u.Estado)
                   .HasColumnName("estado")
                   .HasDefaultValue(true);

            builder.Property(u => u.FechaCreacion)
                   .HasColumnName("fechaCreacion")
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(u => u.CreadoModPor)
                   .HasColumnName("creadoModPor")
                   .IsRequired();

            builder.Property(u => u.UltimaMod)
                   .HasColumnName("ultimaMod")
                   .IsRequired(false);

            // Relaciones
            builder.HasOne(u => u.Persona)
                   .WithOne(p => p.Usuario)
                   .HasForeignKey<Usuario>(u => u.Id)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(u => u.Reservas)
                   .WithOne(r => r.Solicitante)
                   .HasForeignKey(r => r.SolicitanteId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}