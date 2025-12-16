using GestionLlaves.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionLlaves.Data.Configurations
{
    public class HorarioAcademicoConfiguration : IEntityTypeConfiguration<HorarioAcademico>
    {
        public void Configure(EntityTypeBuilder<HorarioAcademico> builder)
        {
            // Nombre de tabla
            builder.ToTable("horarioAcademico");

            // Clave primaria
            builder.HasKey(h => h.Id);
            builder.Property(h => h.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            // Propiedades
            builder.Property(h => h.PeriodoAcademicoId)
                   .HasColumnName("periodoAcademicoId")
                   .IsRequired();

            builder.Property(h => h.MateriaId)
                   .HasColumnName("materiaId")
                   .IsRequired();

            builder.Property(h => h.DocenteId)
                   .HasColumnName("docenteId")
                   .IsRequired();

            builder.Property(h => h.AulaId)
                   .HasColumnName("aulaId")
                   .IsRequired();

            builder.Property(h => h.Lunes)
                   .HasColumnName("lunes")
                   .HasDefaultValue(false);

            builder.Property(h => h.Martes)
                   .HasColumnName("martes")
                   .HasDefaultValue(false);

            builder.Property(h => h.Miercoles)
                   .HasColumnName("miercoles")
                   .HasDefaultValue(false);

            builder.Property(h => h.Jueves)
                   .HasColumnName("jueves")
                   .HasDefaultValue(false);

            builder.Property(h => h.Viernes)
                   .HasColumnName("viernes")
                   .HasDefaultValue(false);

            builder.Property(h => h.Sabado)
                   .HasColumnName("sabado")
                   .HasDefaultValue(false);

            builder.Property(h => h.HoraInicio)
                   .HasColumnName("horaInicio")
                   .HasColumnType("TIME")
                   .IsRequired();

            builder.Property(h => h.HoraFin)
                   .HasColumnName("horaFin")
                   .HasColumnType("TIME")
                   .IsRequired();

            builder.Property(h => h.Grupo)
                   .HasColumnName("grupo")
                   .HasMaxLength(5)
                   .IsRequired();

            builder.Property(h => h.EstadoHorario)
                   .HasColumnName("estadoHorario")
                   .HasDefaultValue(true);

            builder.Property(h => h.Estado)
                   .HasColumnName("estado")
                   .HasDefaultValue(true);

            builder.Property(h => h.FechaCreacion)
                   .HasColumnName("fechaCreacion")
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(a => a.CreadoModPor)
                   .HasColumnName("creadoModPor")
                   .IsRequired();

            builder.Property(h => h.UltimaMod)
                   .HasColumnName("ultimaMod")
                   .IsRequired(false);

            // Relaciones
            builder.HasOne(h => h.PeriodoAcademico)
                   .WithMany(p => p.HorariosAcademicos)
                   .HasForeignKey(h => h.PeriodoAcademicoId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(h => h.Materia)
                   .WithMany(m => m.HorariosAcademicos)
                   .HasForeignKey(h => h.MateriaId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(h => h.Docente)
                   .WithMany(d => d.HorariosAcademicos)
                   .HasForeignKey(h => h.DocenteId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(h => h.Aula)
                   .WithMany(a => a.HorariosAcademicos)
                   .HasForeignKey(h => h.AulaId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(h => h.Prestamos)
                   .WithOne(p => p.HorarioAcademico)
                   .HasForeignKey(p => p.HorarioAcademicoId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}