using Xunit;
using Clarificador.Api.Services;
using System.Threading.Tasks;

namespace Clarificador.Tests;

public class DeepSeekServiceTests
{
    [Fact]
    public async Task ExtraerPrecio_ConTextoDeTikTok_RetornaSoloElPrecio()
    {
        // 1. Arrange
        // IMPORTANTE: Pega tu API Key real aquí SOLO para esta prueba.
        // ADVERTENCIA: Asegúrate de borrarla o no hacer "git commit" con esta llave expuesta.
        string miApiKey = ""; 
        
        var servicioDeepSeek = new DeepSeekService(miApiKey);
        string textoSimulado = "TikTok Shop 2026 - Sudadera con capucha premium color negro. Talla L. Precio de descuento $250.00 MXN. Envío gratis a todo el país. Comprar ahora.";

        // 2. Act
        var precioDetectado = await servicioDeepSeek.AnalizarPrecioAsync(textoSimulado);

        // 3. Assert
        // Validamos que el modelo ignoró la "Sudadera" y el "Envío gratis" y nos dio el dato duro.
        Assert.Contains("250", precioDetectado);
    }
}