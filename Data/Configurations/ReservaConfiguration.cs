using GestionLlaves.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionLlaves.Data.Configurations
{
    public class ReservaConfiguration : IEntityTypeConfiguration<Reserva>
    {
        public void Configure(EntityTypeBuilder<Reserva> builder)
        {
            // Nombre de tabla
            builder.ToTable("reserva");

            // Clave primaria
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            // Propiedades
            builder.Property(r => r.SolicitanteId)
                   .HasColumnName("solicitanteId")
                   .IsRequired();

            builder.Property(r => r.AulaId)
                   .HasColumnName("aulaId")
                   .IsRequired();

            builder.Property(r => r.FechaInicio)
                   .HasColumnName("fechaInicio")
                   .IsRequired();

            builder.Property(r => r.FechaFin)
                   .HasColumnName("fechaFin")
                   .IsRequired();

            builder.Property(r => r.Proposito)
                   .HasColumnName("proposito")
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(r => r.Justificacion)
                   .HasColumnName("justificacion")
                   .HasColumnType("VARCHAR(MAX)")
                   .IsRequired();

            builder.Property(r => r.EstadoReserva)
                   .HasColumnName("estadoReserva")
                   .HasMaxLength(15)
                   .HasDefaultValue("PENDIENTE");

            builder.Property(r => r.AprobadaPor)
                   .HasColumnName("aprobadaPor")
                   .IsRequired(false);

            builder.Property(r => r.FechaAprobacion)
                   .HasColumnName("fechaAprobacion")
                   .IsRequired(false);

            builder.Property(r => r.MotivoRechazo)
                   .HasColumnName("motivoRechazo")
                   .HasColumnType("VARCHAR(MAX)")
                   .IsRequired(false);

            builder.Property(r => r.Estado)
                   .HasColumnName("estado")
                   .HasDefaultValue(true);

            builder.Property(r => r.FechaCreacion)
                   .HasColumnName("fechaCreacion")
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(a => a.CreadoModPor)
                   .HasColumnName("creadoModPor")
                   .IsRequired();

            builder.Property(r => r.UltimaMod)
                   .HasColumnName("ultimaMod")
                   .IsRequired(false);

            // Relaciones
            builder.HasOne(r => r.Solicitante)
                   .WithMany(u => u.Reservas)
                   .HasForeignKey(r => r.SolicitanteId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Aula)
                   .WithMany(a => a.Reservas)
                   .HasForeignKey(r => r.AulaId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(r => r.Prestamos)
                   .WithOne(p => p.Reserva)
                   .HasForeignKey(p => p.ReservaId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.AprobadaPorUsuario)
                   .WithMany()
                   .HasForeignKey(r => r.AprobadaPor)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}