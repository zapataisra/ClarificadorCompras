using System.Text.Json.Serialization;

namespace Clarificador.Api.Models;

public record DeepSeekAnalisis(
    [property: JsonPropertyName("nombre_producto")] string NombreProducto,
    [property: JsonPropertyName("tiene_variantes")] bool TieneVariantes,
    [property: JsonPropertyName("tiene_oferta_relampago")] bool TieneOfertaRelampago,
    [property: JsonPropertyName("tiene_cupon")] bool TieneCupon,
    [property: JsonPropertyName("mensaje_para_usuario")] string MensajeParaUsuario
);