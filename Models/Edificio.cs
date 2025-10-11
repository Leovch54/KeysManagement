using System;
using System.Collections.Generic;

namespace Sistema_Gestion_Llaves.Models;

public partial class Edificio
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Codigo { get; set; } = null!;

    public string? Direccion { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Aula> Aulas { get; set; } = new List<Aula>();
}
