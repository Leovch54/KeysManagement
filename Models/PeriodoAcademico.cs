using System;
using System.Collections.Generic;

namespace Sistema_Gestion_Llaves.Models;

public partial class PeriodoAcademico
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual ICollection<HorarioAcademico> HorarioAcademicos { get; set; } = new List<HorarioAcademico>();
}
