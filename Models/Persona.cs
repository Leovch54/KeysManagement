using System;
using System.Collections.Generic;

namespace Sistema_Gestion_Llaves.Models;

public partial class Persona
{
    public int Id { get; set; }

    public string Nombres { get; set; } = null!;

    public string PrimerApellido { get; set; } = null!;

    public string? SegundoApellido { get; set; }

    public string? Telefono { get; set; }

    public string Tipo { get; set; } = null!;

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual ICollection<HorarioAcademico> HorarioAcademicos { get; set; } = new List<HorarioAcademico>();

    public virtual ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();

    public virtual Usuario? Usuario { get; set; }
}
