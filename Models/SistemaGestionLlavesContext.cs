using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Sistema_Gestion_Llaves.Models;

public partial class SistemaGestionLlavesContext : DbContext
{
    public SistemaGestionLlavesContext()
    {
    }

    public SistemaGestionLlavesContext(DbContextOptions<SistemaGestionLlavesContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Aula> Aulas { get; set; }

    public virtual DbSet<Edificio> Edificios { get; set; }

    public virtual DbSet<HorarioAcademico> HorarioAcademicos { get; set; }

    public virtual DbSet<Materium> Materia { get; set; }

    public virtual DbSet<PeriodoAcademico> PeriodoAcademicos { get; set; }

    public virtual DbSet<Persona> Personas { get; set; }

    public virtual DbSet<Prestamo> Prestamos { get; set; }

    public virtual DbSet<Reserva> Reservas { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-6USK67I;Database=SistemaGestionLlaves;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Aula>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Aula__3214EC0742E5B64D");

            entity.ToTable("Aula");

            entity.HasIndex(e => e.Codigo, "UQ__Aula__06370DAC62695646").IsUnique();

            entity.Property(e => e.Activa).HasDefaultValue(true);
            entity.Property(e => e.Codigo)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("DISPONIBLE");
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Edificio).WithMany(p => p.Aulas)
                .HasForeignKey(d => d.EdificioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Aula_Edificio");
        });

        modelBuilder.Entity<Edificio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Edificio__3214EC072E18EF03");

            entity.ToTable("Edificio");

            entity.HasIndex(e => e.Codigo, "UQ__Edificio__06370DACF07DB3B9").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Codigo)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<HorarioAcademico>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HorarioA__3214EC07D1A3C8FE");

            entity.ToTable("HorarioAcademico");

            entity.Property(e => e.Dia)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasDefaultValue("ACTIVO");
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Aula).WithMany(p => p.HorarioAcademicos)
                .HasForeignKey(d => d.AulaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Horario_Aula");

            entity.HasOne(d => d.Docente).WithMany(p => p.HorarioAcademicos)
                .HasForeignKey(d => d.DocenteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Horario_Docente");

            entity.HasOne(d => d.Materia).WithMany(p => p.HorarioAcademicos)
                .HasForeignKey(d => d.MateriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Horario_Materia");

            entity.HasOne(d => d.PeriodoAcademico).WithMany(p => p.HorarioAcademicos)
                .HasForeignKey(d => d.PeriodoAcademicoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Horario_Periodo");
        });

        modelBuilder.Entity<Materium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Materia__3214EC07D3985354");

            entity.Property(e => e.Activa).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PeriodoAcademico>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PeriodoA__3214EC0706757A56");

            entity.ToTable("PeriodoAcademico");

            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Persona>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Persona__3214EC07F033DEDF");

            entity.ToTable("Persona");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Nombres)
                .HasMaxLength(60)
                .IsUnicode(false);
            entity.Property(e => e.PrimerApellido)
                .HasMaxLength(60)
                .IsUnicode(false);
            entity.Property(e => e.SegundoApellido)
                .HasMaxLength(60)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Tipo)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Prestamo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Prestamo__3214EC07CB103BFE");

            entity.ToTable("Prestamo");

            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasDefaultValue("ACTIVO");
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FechaFinProgramada).HasPrecision(3);
            entity.Property(e => e.FechaFinReal).HasPrecision(3);
            entity.Property(e => e.FechaInicio).HasPrecision(3);
            entity.Property(e => e.Tipo)
                .HasMaxLength(15)
                .IsUnicode(false);

            entity.HasOne(d => d.Aula).WithMany(p => p.Prestamos)
                .HasForeignKey(d => d.AulaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Prestamo_Aula");

            entity.HasOne(d => d.DevueltoPorNavigation).WithMany(p => p.Prestamos)
                .HasForeignKey(d => d.DevueltoPor)
                .HasConstraintName("FK_Prestamo_DevueltoPor");

            entity.HasOne(d => d.Persona).WithMany(p => p.Prestamos)
                .HasForeignKey(d => d.PersonaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Prestamo_Persona");
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reserva__3214EC07421ED5C6");

            entity.ToTable("Reserva");

            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasDefaultValue("PENDIENTE");
            entity.Property(e => e.FechaAprobacion).HasPrecision(3);
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FechaFin).HasPrecision(3);
            entity.Property(e => e.FechaInicio).HasPrecision(3);
            entity.Property(e => e.Proposito)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.AprobadaPorNavigation).WithMany(p => p.ReservaAprobadaPorNavigations)
                .HasForeignKey(d => d.AprobadaPor)
                .HasConstraintName("FK_Reserva_AprobadaPor");

            entity.HasOne(d => d.Aula).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.AulaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reserva_Aula");

            entity.HasOne(d => d.Usuario).WithMany(p => p.ReservaUsuarios)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reserva_Usuario");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuario__3214EC07D9414637");

            entity.ToTable("Usuario");

            entity.HasIndex(e => e.Email, "UQ__Usuario__A9D10534B8C1AAE7").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FechaUltimaConexion).HasPrecision(3);
            entity.Property(e => e.Pin)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Rol)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Usuario)
                .HasForeignKey<Usuario>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuario_Persona");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
