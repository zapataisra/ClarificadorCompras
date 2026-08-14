using Microsoft.AspNetCore.Mvc;

namespace Clarificador.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WhatsAppWebhookController : ControllerBase
    {
        // Este es el token secreto que inventaremos nosotros.
        // Cópialo porque lo pegaremos en la página de Meta más adelante.
        private readonly string _verifyToken = "TokenSeguroTikTokBot2026"; 

        // 1. El método GET: Sirve ÚNICAMENTE para que Meta verifique que esta API es nuestra
        [HttpGet]
        public IActionResult VerifyWebhook([FromQuery(Name = "hub.mode")] string mode,
                                           [FromQuery(Name = "hub.challenge")] string challenge,
                                           [FromQuery(Name = "hub.verify_token")] string token)
        {
            if (mode == "subscribe" && token == _verifyToken)
            {
                // Si el token coincide, Meta nos pide que le devolvamos el "challenge" exacto
                return Ok(challenge);
            }
            return Forbid();
        }

        // 2. El método POST: Aquí es donde llegarán los mensajes reales de WhatsApp (los enlaces de TikTok)
        [HttpPost]
        public IActionResult ReceiveMessage([FromBody] object body)
        {
            // Regla de oro de la arquitectura de Webhooks: 
            // Siempre responde un HTTP 200 (Ok) INMEDIATAMENTE para que Meta no crea que tu servidor se cayó.
            // El procesamiento del enlace y la IA lo haremos en segundo plano.
            return Ok();
        }
    }
}