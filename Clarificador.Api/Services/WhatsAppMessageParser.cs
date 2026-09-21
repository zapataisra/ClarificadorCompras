using System.Text.Json;
using System.Text.RegularExpressions;

namespace Clarificador.Api.Services;

public class WhatsAppMessageParser
{
    public string ExtraerEnlaceDeMensaje(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var textBody = root
                .GetProperty("entry")[0]
                .GetProperty("changes")[0]
                .GetProperty("value")
                .GetProperty("messages")[0]
                .GetProperty("text")
                .GetProperty("body").GetString();

            if (string.IsNullOrEmpty(textBody))
                return string.Empty;

            // Expresión regular actualizada para aceptar dominios cortos (vt. / vm.) y largos de TikTok
            var match = Regex.Match(textBody, @"https?://(?:www\.)?(?:tiktok\.com|vt\.tiktok\.com|vm\.tiktok\.com)/\S+", RegexOptions.IgnoreCase);
            
            return match.Success ? match.Value : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
    public string ExtraerNumeroRemitente(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var numero = root
                .GetProperty("entry")[0]
                .GetProperty("changes")[0]
                .GetProperty("value")
                .GetProperty("messages")[0]
                .GetProperty("from")
                .GetString();

            return numero ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}