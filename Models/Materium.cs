using System;
using System.Collections.Generic;

namespace Sistema_Gestion_Llaves.Models;

public partial class Materium
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public bool Activa { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual ICollection<HorarioAcademico> HorarioAcademicos { get; set; } = new List<HorarioAcademico>();
}
