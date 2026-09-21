using Xunit;
using Clarificador.Api.Services;
using Clarificador.Api.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Clarificador.Tests;

public class DeepSeekServiceTests
{
    [Fact]
    public async Task AnalizarAsync_ConTextoDeTikTok_AplicaReglasDeOferta()
    {
        // 1. Arrange
        string miApiKey = "TU_API_KEY_AQUI"; // Borra esto antes del próximo commit
        
        var inMemorySettings = new Dictionary<string, string?> {
            {"DeepSeek:ApiKey", miApiKey}
        };
        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var httpClient = new HttpClient();
        var servicioDeepSeek = new DeepSeekService(httpClient, config);
        
        // Ahora pasamos un ScrapedData como exige tu nueva arquitectura
        // Reemplaza la instancia de datosSimulados con esto:
        var datosSimulados = new ScrapedData(
            PrecioBase: "$250.00",
            TextoPagina: "TikTok Shop 2026 - Sudadera con capucha premium color negro. Talla (3). Oferta relámpago $250.00 MXN."
        );
        // 2. Act
        var resultado = await servicioDeepSeek.AnalizarAsync(datosSimulados);

        // 3. Assert
        Assert.NotNull(resultado);
        Assert.NotNull(resultado.MensajeParaUsuario);
        // Validamos que detectó la táctica de escasez (Oferta relámpago) según tus reglas de DeepSeek
        Assert.True(resultado.TieneOfertaRelampago);
    }
}