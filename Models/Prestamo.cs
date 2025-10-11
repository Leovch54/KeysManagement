using System;
using System.Collections.Generic;

namespace Sistema_Gestion_Llaves.Models;

public partial class Prestamo
{
    public int Id { get; set; }

    public int PersonaId { get; set; }

    public int AulaId { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFinProgramada { get; set; }

    public DateTime? FechaFinReal { get; set; }

    public string Tipo { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public int RegistradoPor { get; set; }

    public int? DevueltoPor { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual Aula Aula { get; set; } = null!;

    public virtual Usuario? DevueltoPorNavigation { get; set; }

    public virtual Persona Persona { get; set; } = null!;
}
