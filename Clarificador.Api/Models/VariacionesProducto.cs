using System;
using System.Collections.Generic;

namespace Clarificador.Api.Models;

public partial class VariacionesProducto
{
    public int Id { get; set; }

    public int? ProductoId { get; set; }

    public string? NombreVariacion { get; set; }

    public decimal? PrecioReal { get; set; }

    public virtual Producto? Producto { get; set; }
}
