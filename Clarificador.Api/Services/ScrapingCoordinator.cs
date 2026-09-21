using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Clarificador.Api.Models;

namespace Clarificador.Api.Services;

public class ScrapingCoordinator
{
    private readonly ConcurrentDictionary<string, Lazy<Task<ProductoAnalisis>>> _enVuelo = new();
    private readonly IMemoryCache _cache;
    private readonly IServiceScopeFactory _scopeFactory;


    public ScrapingCoordinator(IMemoryCache cache, IServiceScopeFactory scopeFactory)
    {
        _cache = cache;
        _scopeFactory = scopeFactory;

    }

    public Task<ProductoAnalisis> ObtenerOAnalizarAsync(string urlNormalizada)
    {
        if (_cache.TryGetValue(urlNormalizada, out ProductoAnalisis? cacheado))
            return Task.FromResult(cacheado!);

        var lazy = _enVuelo.GetOrAdd(urlNormalizada, _ =>
            new Lazy<Task<ProductoAnalisis>>(
                () => EjecutarAnalisisAsync(urlNormalizada),
                LazyThreadSafetyMode.ExecutionAndPublication));

        return lazy.Value;
    }
    private async Task<ProductoAnalisis> EjecutarAnalisisAsync(string url)
    {
        try
        {
            // 1. Abrimos una cápsula de tiempo segura (Scope)
            using var scope = _scopeFactory.CreateScope();
            
            // 2. Extraemos las herramientas frescas desde la cápsula
            var scraper = scope.ServiceProvider.GetRequiredService<TikTokScraperService>();
            var deepSeek = scope.ServiceProvider.GetRequiredService<DeepSeekService>();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // =====================================================================
            // 3. LA CIRUGÍA: EL DOBLE TAP (Escudo Anti-CAPTCHA)
            // =====================================================================
            int maxReintentos = 2;
            ScrapeResult scrapeResult = null!;

            for (int intento = 1; intento <= maxReintentos; intento++)
            {
                Console.WriteLine($"[Scraper] Intentando extraer texto (Intento {intento}/{maxReintentos})...");
                
                scrapeResult = await scraper.ExtraerTextoDePaginaAsync(url);
                
                // Revisamos si Python gritó que hay un CAPTCHA
                if (scrapeResult.ToString()?.Contains("\"status\": \"captcha_detected\"", StringComparison.Ordinal) == true)
                {
                    Console.WriteLine($"[Scraper] ⚠️ CAPTCHA detectado en intento {intento}.");
                    
                    if (intento < maxReintentos)
                    {
                        await Task.Delay(2000); // Pausa táctica antes de volver a golpear
                        continue; 
                    }
                    else
                    {
                        // Si falló 2 veces, abortamos y le mandamos el mensaje al usuario en WhatsApp
                        return new ProductoAnalisis(
                            0, // ID 0 porque no se guardó en BD
                            "Acceso Bloqueado", 
                            null, 
                            null, 
                            false, 
                            false,
                            "🚨 TikTok ha bloqueado temporalmente mi lectura por seguridad. Por favor, intenta enviarme el enlace de nuevo en un par de minutos."
                        );
                    }
                }
                
                // Si llegó aquí, el scraping fue exitoso, rompemos el ciclo
                break; 
            }
            // =====================================================================
            
            // 4. Continuamos con tu flujo normal usando el scrapeResult limpio
            var datos = scraper.ParsearDatos(scrapeResult);
            var analisisIA = await deepSeek.AnalizarAsync(datos);
            var precioDecimal = ParsearPrecio(datos.PrecioBase);

            var producto = await db.Productos.FirstOrDefaultAsync(p => p.UrlNormalizada == url)
                ?? new Producto { UrlNormalizada = url, UrlProducto = url };

            producto.NombreProducto = analisisIA.NombreProducto;
            producto.UltimoPrecioBase = precioDecimal;
            producto.TieneOfertaRelampago = analisisIA.TieneOfertaRelampago;
            producto.TieneCupon = analisisIA.TieneCupon;
            producto.UltimaActualizacion = DateTime.UtcNow;

            if (producto.Id == 0) db.Productos.Add(producto);
            await db.SaveChangesAsync();

            var resultado = new ProductoAnalisis(
                producto.Id, 
                analisisIA.NombreProducto, 
                precioDecimal,
                null, 
                analisisIA.TieneOfertaRelampago, 
                analisisIA.TieneCupon,
                analisisIA.MensajeParaUsuario);

            var ttl = analisisIA.TieneOfertaRelampago ? TimeSpan.FromSeconds(60) : TimeSpan.FromMinutes(3);
            _cache.Set(url, resultado, ttl);
            
            return resultado;
        }
        finally
        {
            _enVuelo.TryRemove(url, out _);
        }
    }

    private async Task<ProductoAnalisis> EjecutarAnalisisAsync2(string url)
    {
        try
        {
            // 1. Abrimos una cápsula de tiempo segura (Scope)
            using var scope = _scopeFactory.CreateScope();
            
            // 2. Extraemos las herramientas frescas desde la cápsula
            var scraper = scope.ServiceProvider.GetRequiredService<TikTokScraperService>();
            var deepSeek = scope.ServiceProvider.GetRequiredService<DeepSeekService>();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // 3. Trabajamos con las herramientas (nota que ya no tienen el "_" al inicio)
            var scrapeResult = await scraper.ExtraerTextoDePaginaAsync(url);
            var datos = scraper.ParsearDatos(scrapeResult);
            
            var analisisIA = await deepSeek.AnalizarAsync(datos);
            var precioDecimal = ParsearPrecio(datos.PrecioBase);

            var producto = await db.Productos.FirstOrDefaultAsync(p => p.UrlNormalizada == url)
                ?? new Producto { UrlNormalizada = url, UrlProducto = url };

            producto.NombreProducto = analisisIA.NombreProducto;
            producto.UltimoPrecioBase = precioDecimal;
            producto.TieneOfertaRelampago = analisisIA.TieneOfertaRelampago;
            producto.TieneCupon = analisisIA.TieneCupon;
            producto.UltimaActualizacion = DateTime.UtcNow;

            if (producto.Id == 0) db.Productos.Add(producto);
            await db.SaveChangesAsync();

            var resultado = new ProductoAnalisis(
                producto.Id, 
                analisisIA.NombreProducto, 
                precioDecimal,
                null, 
                analisisIA.TieneOfertaRelampago, 
                analisisIA.TieneCupon,
                analisisIA.MensajeParaUsuario);

            var ttl = analisisIA.TieneOfertaRelampago ? TimeSpan.FromSeconds(60) : TimeSpan.FromMinutes(3);
            _cache.Set(url, resultado, ttl);
            
            return resultado;
        }
        finally
        {
            _enVuelo.TryRemove(url, out _);
        }
    }

    private static decimal? ParsearPrecio(string precioTexto)
    {
        var limpio = Regex.Replace(precioTexto, @"[^\d.]", "");
        return decimal.TryParse(limpio, out var valor) ? valor : null;
    }
}