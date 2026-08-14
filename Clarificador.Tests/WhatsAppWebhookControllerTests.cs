using Microsoft.AspNetCore.Mvc;
using Clarificador.Api.Controllers;
using Xunit;

namespace Clarificador.Tests
{
    public class WhatsAppWebhookControllerTests
    {
        [Fact]
        public void VerifyWebhook_ConTokenIncorrecto_RetornaForbid()
        {
            // 1. Arrange (Preparar el escenario)
            var controller = new WhatsAppWebhookController();
            var modo = "subscribe";
            var challenge = "123456789";
            var tokenIncorrecto = "TokenFalsoHackerman";

            // 2. Act (Ejecutar la acción a probar)
            var result = controller.VerifyWebhook(modo, challenge, tokenIncorrecto);

            // 3. Assert (Verificar el resultado esperado)
            // Esperamos que el resultado sea un ForbidResult (HTTP 403) porque el token no coincide
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public void VerifyWebhook_ConTokenCorrecto_RetornaOkConChallenge()
        {
            // Arrange
            var controller = new WhatsAppWebhookController();
            var modo = "subscribe";
            var challenge = "123456789";
            var tokenCorrecto = "TokenSeguroTikTokBot2026"; // El token exacto que pusiste en tu API

            // Act
            var result = controller.VerifyWebhook(modo, challenge, tokenCorrecto);

            // Assert
            // Esperamos que devuelva un HTTP 200 (OkObjectResult) y que el contenido sea el challenge
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(challenge, okResult.Value);
        }
    }
}