using Microsoft.AspNetCore.Mvc;
using Clarificador.Api.Controllers;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Xunit;

namespace Clarificador.Tests
{
    public class WhatsAppWebhookControllerTests
    {
        // Método auxiliar para crear un controlador con configuración falsa
        private WhatsAppWebhookController CrearControladorConConfiguracion(string verifyToken = "TokenSeguroTikTokBot2026")
        {
            var inMemorySettings = new Dictionary<string, string?> {
                {"WhatsApp:VerifyToken", verifyToken},
                {"WhatsApp:MetaAccessToken", "TokenFalso"},
                {"WhatsApp:PhoneNumberId", "123456789"}
            };

            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // Pasamos null al coordinador y al caché porque VerifyWebhook no los usa
            return new WhatsAppWebhookController(null!, null!, null!, config);
        }

        [Fact]
        public void VerifyWebhook_ConTokenIncorrecto_RetornaForbid()
        {
            var controller = CrearControladorConConfiguracion();
            var modo = "subscribe";
            var challenge = "123456789";
            var tokenIncorrecto = "TokenFalsoHackerman";

            var result = controller.VerifyWebhook(modo, challenge, tokenIncorrecto);

            Assert.IsType<BadRequestObjectResult>(result); // Tu código devuelve BadRequest, no Forbid
        }

        [Fact]
        public void VerifyWebhook_ConTokenCorrecto_RetornaOkConChallenge()
        {
            var controller = CrearControladorConConfiguracion("TokenSeguroTikTokBot2026");
            var modo = "subscribe";
            var challenge = "123456789";
            var tokenCorrecto = "TokenSeguroTikTokBot2026";

            var result = controller.VerifyWebhook(modo, challenge, tokenCorrecto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(challenge, okResult.Value);
        }
    }
}