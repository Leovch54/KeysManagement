using System;
using System.Collections.Generic;

namespace Sistema_Gestion_Llaves.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string Pin { get; set; } = null!;

    public string Rol { get; set; } = null!;

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaUltimaConexion { get; set; }
    public bool PrimeraVez {  get; set; }

    public virtual Persona IdNavigation { get; set; } = null!;

    public virtual ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();

    public virtual ICollection<Reserva> ReservaAprobadaPorNavigations { get; set; } = new List<Reserva>();

    public virtual ICollection<Reserva> ReservaUsuarios { get; set; } = new List<Reserva>();
}
