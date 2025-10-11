using System;
using System.Collections.Generic;

namespace Sistema_Gestion_Llaves.Models;

public partial class Aula
{
    public int Id { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public int EdificioId { get; set; }

    public int? Piso { get; set; }

    public int? Capacidad { get; set; }

    public bool TieneProyector { get; set; }

    public bool TieneTv { get; set; }

    public string Estado { get; set; } = null!;

    public bool Activa { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual Edificio Edificio { get; set; } = null!;

    public virtual ICollection<HorarioAcademico> HorarioAcademicos { get; set; } = new List<HorarioAcademico>();

    public virtual ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
