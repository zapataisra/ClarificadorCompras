using System.Text.Json.Serialization;

namespace Clarificador.Api.Models;

public record ScrapedData(
    [property: JsonPropertyName("precio_base")] string PrecioBase,
    [property: JsonPropertyName("texto_pagina")] string TextoPagina
);