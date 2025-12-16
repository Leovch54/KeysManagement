using GestionLlaves.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionLlaves.Data.Configurations
{
    public class NotificacionConfiguration : IEntityTypeConfiguration<Notificacion>
    {
        public void Configure(EntityTypeBuilder<Notificacion> builder)
        {
            // Nombre de tabla
            builder.ToTable("notificacion");

            // Clave primaria
            builder.HasKey(n => n.Id);
            builder.Property(n => n.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            // Propiedades
            builder.Property(n => n.UsuarioId)
                   .HasColumnName("usuarioId")
                   .IsRequired();

            builder.Property(n => n.Tipo)
                   .HasColumnName("tipo")
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(n => n.Mensaje)
                   .HasColumnName("mensaje")
                   .HasColumnType("VARCHAR(MAX)")
                   .IsRequired();

            builder.Property(n => n.Prioridad)
                   .HasColumnName("prioridad")
                   .HasMaxLength(20)
                   .HasDefaultValue("MEDIA")
                   .IsRequired();

            builder.Property(n => n.PrestamoId)
                   .HasColumnName("prestamoId")
                   .IsRequired(false);

            builder.Property(n => n.ReservaId)
                   .HasColumnName("reservaId")
                   .IsRequired(false);

            builder.Property(n => n.HorarioAcademicoId)
                   .HasColumnName("horarioAcademicoId")
                   .IsRequired(false);

            builder.Property(n => n.Leida)
                   .HasColumnName("leida")
                   .HasDefaultValue(false)
                   .IsRequired();

            builder.Property(n => n.FechaLectura)
                   .HasColumnName("fechaLectura")
                   .IsRequired(false);

            builder.Property(n => n.FechaCreacion)
                   .HasColumnName("fechaCreacion")
                   .HasDefaultValueSql("GETDATE()")
                   .IsRequired();

            builder.Property(n => n.FechaProgramada)
                   .HasColumnName("fechaProgramada")
                   .IsRequired(false);

            builder.Property(n => n.FechaEnvio)
                   .HasColumnName("fechaEnvio")
                   .IsRequired(false);

            builder.Property(n => n.Estado)
                   .HasColumnName("estado")
                   .HasDefaultValue(true)
                   .IsRequired();

            // Relaciones
            builder.HasOne(n => n.Usuario)
                   .WithMany()
                   .HasForeignKey(n => n.UsuarioId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(n => n.Prestamo)
                   .WithMany()
                   .HasForeignKey(n => n.PrestamoId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(n => n.Reserva)
                   .WithMany()
                   .HasForeignKey(n => n.ReservaId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(n => n.HorarioAcademico)
                   .WithMany()
                   .HasForeignKey(n => n.HorarioAcademicoId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}