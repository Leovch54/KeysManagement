using System;
using System.Collections.Generic;

namespace Sistema_Gestion_Llaves.Models;

public partial class HorarioAcademico
{
    public int Id { get; set; }

    public int PeriodoAcademicoId { get; set; }

    public int MateriaId { get; set; }

    public int DocenteId { get; set; }

    public int AulaId { get; set; }

    public string Dia { get; set; } = null!;

    public TimeOnly HoraInicio { get; set; }

    public TimeOnly HoraFin { get; set; }

    public string Estado { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public virtual Aula Aula { get; set; } = null!;

    public virtual Persona Docente { get; set; } = null!;

    public virtual Materium Materia { get; set; } = null!;

    public virtual PeriodoAcademico PeriodoAcademico { get; set; } = null!;
}
