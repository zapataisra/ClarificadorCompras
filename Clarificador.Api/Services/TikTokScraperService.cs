using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Clarificador.Api.Models;
using Clarificador.Api.Exceptions;

namespace Clarificador.Api.Services;

// Esta es la estructura que recibirá la respuesta de Python
public record ScrapeResult(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("text")] string? Text,
    [property: JsonPropertyName("error")] string? Error);

public class TikTokScraperService
{
    private const int TimeoutSeconds = 90;

    public async Task<ScrapeResult> ExtraerTextoDePaginaAsync(string url)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "python",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8
        };
        psi.ArgumentList.Add("francotirador.py");
        psi.ArgumentList.Add(url); 

        using var process = new Process { StartInfo = psi };
        process.Start();

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(TimeoutSeconds));
        try
        {
            await process.WaitForExitAsync(cts.Token);
            var stdout = await stdoutTask;
            var stderr = await stderrTask;

            if (!string.IsNullOrWhiteSpace(stderr))
                Console.WriteLine($"[Python stderr]: {stderr}");

            return JsonSerializer.Deserialize<ScrapeResult>(stdout)
                   ?? new ScrapeResult("error", null, "Respuesta vacía de Python");
        }
        catch (OperationCanceledException)
        {
            try { process.Kill(entireProcessTree: true); } catch { }
            return new ScrapeResult("error", null, "Timeout: el scraping tardó más de 30s");
        }
    }

    // NUEVO: convierte el ScrapeResult en los datos estructurados que usa el coordinador
   public ScrapedData ParsearDatos(ScrapeResult resultado)
    {
        if (resultado.Status == "captcha_detected")
            throw new CaptchaDetectedException();
        if (resultado.Status != "success" || resultado.Text is null)
            throw new ScrapingFailedException(resultado.Error ?? "Error desconocido en el scraping");

        return JsonSerializer.Deserialize<ScrapedData>(resultado.Text)
            ?? throw new InvalidOperationException("No se pudo parsear el JSON de datos scrapeados");
    }

}