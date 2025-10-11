using System;
using System.Collections.Generic;

namespace Sistema_Gestion_Llaves.Models;

public partial class Reserva
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public int AulaId { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public string Proposito { get; set; } = null!;

    public string Justificacion { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public int? AprobadaPor { get; set; }

    public DateTime? FechaAprobacion { get; set; }

    public string? MotivoRechazo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual Usuario? AprobadaPorNavigation { get; set; }

    public virtual Aula Aula { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;
}
