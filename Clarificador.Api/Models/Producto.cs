using System;
using System.Collections.Generic;

namespace Clarificador.Api.Models;

public partial class Producto
{
    public int Id { get; set; }

    public string UrlProducto { get; set; } = null!;

    public string UrlNormalizada { get; set; } = null!;

    public string? NombreProducto { get; set; }

    public decimal? UltimoPrecioBase { get; set; }

    public bool? TieneOfertaRelampago { get; set; }

    public bool? TieneCupon { get; set; }

    public DateTime? UltimaActualizacion { get; set; }

    public virtual ICollection<ConsultasUsuario> ConsultasUsuarios { get; set; } = new List<ConsultasUsuario>();

    public virtual ICollection<VariacionesProducto> VariacionesProductos { get; set; } = new List<VariacionesProducto>();
}
