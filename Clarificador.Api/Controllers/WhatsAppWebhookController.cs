using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Clarificador.Api.Services;
using Clarificador.Api.Models;
using Clarificador.Api.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using System.Text;

namespace Clarificador.Api.Controllers
{
    [ApiController]
    [Route("api/webhook")]
    public class WhatsAppWebhookController : ControllerBase
    {
        private readonly string _verifyToken; 
        private readonly string _metaAccessToken;
        private readonly string _phoneNumberId;
        private readonly HttpClient _httpClient;
        private readonly WhatsAppMessageParser _parser = new();
        // entra _coordinator (ya los usa a ambos por dentro, más caché y "en vuelo")
        private readonly ScrapingCoordinator _coordinator;      
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMemoryCache _cache;

        public WhatsAppWebhookController(ScrapingCoordinator coordinator, IServiceScopeFactory scopeFactory,IMemoryCache cache, IConfiguration configuracion)
        {
            _verifyToken = configuracion["WhatsApp:VerifyToken"] ?? throw new Exception("El token de verificación de WhatsApp no está configurado.");
            _metaAccessToken = configuracion["WhatsApp:MetaAccessToken"] ?? throw new Exception("El token de acceso de Meta no está configurado.");
            _phoneNumberId = configuracion["WhatsApp:PhoneNumberId"] ?? throw new Exception("El ID del número de teléfono de WhatsApp no está configurado.");

            _coordinator = coordinator;
            _scopeFactory = scopeFactory;
            _cache = cache;
            _httpClient = new HttpClient();
        }

        // 1. VerifyWebhook — SIN CAMBIOS, exactamente como lo tenías
        [HttpGet]
        public IActionResult VerifyWebhook([FromQuery(Name = "hub.mode")] string? mode,
                                           [FromQuery(Name = "hub.challenge")] string? challenge,
                                           [FromQuery(Name = "hub.verify_token")] string? token)
        {
            if (mode == "subscribe" && token == _verifyToken)
            {
                return Ok(challenge);
            }
            return BadRequest("Petición inválida o token incorrecto.");
        }

        // 2. ReceiveMessage — SIN CAMBIOS, exactamente como lo tenías
        [HttpPost]
        public async Task<IActionResult> ReceiveMessage([FromBody] JsonElement body)
        {
            try
            {
                var jsonString = body.GetRawText();
                
                // 1. Usamos el parser para sacar datos limpios
                var fromNumber = _parser.ExtraerNumeroRemitente(jsonString);
                var urlLimpia = _parser.ExtraerEnlaceDeMensaje(jsonString);

                if (string.IsNullOrEmpty(fromNumber)) return Ok();

                // 2. Normalización del número de México (El escudo anti-Error 131030)
                if (fromNumber.StartsWith("521") && fromNumber.Length == 13)
                {
                    fromNumber = "52" + fromNumber.Substring(3);
                }
                // 3. Pasamos SIEMPRE la tarea al segundo plano. 
                // El procesador decidirá si lo analiza o si le aplica el escudo.
                _ = Task.Run(async () => await ProcesarYResponderAsync(fromNumber, urlLimpia));
                // 3. Solo pasamos al coordinador si realmente hay un enlace limpio
                //if (!string.IsNullOrEmpty(urlLimpia))
                //{
                //    Console.WriteLine($"\n[WHATSAPP] Enlace limpio recibido de {fromNumber}: {urlLimpia}");
                //    _ = Task.Run(async () => await ProcesarYResponderAsync(fromNumber, urlLimpia));
                //}

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error de Webhook]: {ex.Message}");
                return Ok();
            }
        }

        // 3. EL CEREBRO — este es el único método con cambios reales
        private async Task ProcesarYResponderAsync(string numeroUsuario, string urlLimpia)
        {
            // Creamos un mini-universo seguro para la base de datos en segundo plano
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            try
            {
                Console.WriteLine($"\n[WHATSAPP] Enlace limpio recibido de {numeroUsuario}: {urlLimpia}");
                // EL ESCUDO FINANCIERO DEFINITIVO (Cero enlaces falsos)
                if (string.IsNullOrEmpty(urlLimpia))
                {
                    // Si no hay enlace válido de TikTok, revisamos si ya lo educamos hoy
                    if (!_cache.TryGetValue($"Aviso_{numeroUsuario}", out _))
                    {
                        await EnviarMensajeWhatsApp(numeroUsuario, "Por favor, mándame un enlace válido de TikTok Shop para analizarlo.");
                        _cache.Set($"Aviso_{numeroUsuario}", true, TimeSpan.FromHours(24));
                    }
                    else
                    {
                        Console.WriteLine($"[WHATSAPP] Visto táctico aplicado a {numeroUsuario}. Enlace no era de TikTok o es spam.");
                    }
                    return; // Abortamos la misión aquí mismo.
                }

                // Si llegó hasta aquí, urlLimpia es 100% de TikTok. Arrancamos motores.
                await EnviarMensajeWhatsApp(numeroUsuario, "⏳ Analizando el producto, dame unos segundos...");

                var resultado = await _coordinator.ObtenerOAnalizarAsync(urlLimpia);

                db.ConsultasUsuarios.Add(new ConsultasUsuario
                {
                    TelefonoUsuario = numeroUsuario,
                    ProductoId = resultado.ProductoId,
                    Estado = "completado",
                    FechaConsulta = DateTime.UtcNow
                });
                await db.SaveChangesAsync();

                await EnviarMensajeWhatsApp(numeroUsuario, resultado.MensajeParaUsuario);
            }
            catch (CaptchaDetectedException)
            {
                db.ConsultasUsuarios.Add(new ConsultasUsuario
                {
                    TelefonoUsuario = numeroUsuario,
                    Estado = "fallido",
                    MotivoFallo = "captcha_detectado",
                    FechaConsulta = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
                await EnviarMensajeWhatsApp(numeroUsuario, "No pude revisar ese producto en este momento, puede ser algo temporal. ¿Lo intentamos de nuevo en unos minutos?");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR INTERNO]: {ex.Message}");
                db.ConsultasUsuarios.Add(new ConsultasUsuario
                {
                    TelefonoUsuario = numeroUsuario,
                    Estado = "fallido",
                    MotivoFallo = ex.Message,
                    FechaConsulta = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
                await EnviarMensajeWhatsApp(numeroUsuario, "Lo siento, tuve un problema procesando ese enlace. ¿Puedes intentar con otro?");
            }
        }

        // 4. EnviarMensajeWhatsApp — SIN CAMBIOS, exactamente como lo tenías
        private async Task EnviarMensajeWhatsApp(string numeroDestino, string textoMensaje)
        {
            var url = $"https://graph.facebook.com/v19.0/{_phoneNumberId}/messages";

            var payload = new
            {
                messaging_product = "whatsapp",
                to = numeroDestino,
                type = "text",
                text = new { body = textoMensaje }
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_metaAccessToken}");

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ERROR META]: {error}");
            }
            else
            {
                Console.WriteLine($"[WHATSAPP] Respuesta enviada a {numeroDestino}");
            }
        }
    }
}