using System;
using System.Collections.Generic;

namespace Clarificador.Api.Models;

public partial class ConsultasUsuario
{
    public int Id { get; set; }

    public string? TelefonoUsuario { get; set; }

    public int? ProductoId { get; set; }

    public string? Estado { get; set; }

    public string? MotivoFallo { get; set; }

    public DateTime? FechaConsulta { get; set; }

    public virtual Producto? Producto { get; set; }
}
