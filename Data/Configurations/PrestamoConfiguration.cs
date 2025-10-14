using GestionLlaves.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionLlaves.Data.Configurations
{
    public class PrestamoConfiguration : IEntityTypeConfiguration<Prestamo>
    {
        public void Configure(EntityTypeBuilder<Prestamo> builder)
        {
            // Nombre de tabla
            builder.ToTable("prestamo");

            // Clave primaria
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            // Propiedades
            builder.Property(p => p.PersonaId)
                   .HasColumnName("personaId")
                   .IsRequired();

            builder.Property(p => p.AulaId)
                   .HasColumnName("aulaId")
                   .IsRequired();

            builder.Property(p => p.FechaInicio)
                   .HasColumnName("fechaInicio")
                   .IsRequired();

            builder.Property(p => p.FechaFinProgramada)
                   .HasColumnName("fechaFinProgramada")
                   .IsRequired();

            builder.Property(p => p.FechaFinReal)
                   .HasColumnName("fechaFinReal")
                   .IsRequired(false);

            builder.Property(p => p.Tipo)
                   .HasColumnName("tipo")
                   .HasMaxLength(15)
                   .IsRequired();

            builder.Property(p => p.EstadoPrestamo)
                   .HasColumnName("estadoPrestamo")
                   .HasMaxLength(15)
                   .HasDefaultValue("ACTIVO");

            builder.Property(p => p.RecibidoPor)
                   .HasColumnName("recibidoPor")      
                   .IsRequired(false);

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

            builder.Property(p => p.ReservaId)
                   .HasColumnName("reservaId")
                   .IsRequired(false);

            builder.Property(p => p.HorarioAcademicoId)
                   .HasColumnName("horarioAcademicoId")
                   .IsRequired(false);

            // Realciones

            builder.HasOne(p => p.Persona)
                   .WithMany(per => per.Prestamos)
                   .HasForeignKey(p => p.PersonaId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Aula)
                   .WithMany(a => a.Prestamos)
                   .HasForeignKey(p => p.AulaId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Reserva)
                   .WithMany(r => r.Prestamos)
                   .HasForeignKey(p => p.ReservaId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.HorarioAcademico)
                   .WithMany(h => h.Prestamos)
                   .HasForeignKey(p => p.HorarioAcademicoId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.RecibidoPorUsuario)
                   .WithMany()
                   .HasForeignKey(p => p.RecibidoPor)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}