namespace Clarificador.Api.Models;

public record ProductoAnalisis(
    int ProductoId,
    string NombreProducto,
    decimal? PrecioBase,
    string? RangoPrecio,
    bool TieneOfertaRelampago,
    bool TieneCupon,
    string MensajeParaUsuario
);