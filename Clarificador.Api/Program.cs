using Clarificador.Api.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<Clarificador.Api.Services.TikTokScraperService>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<Clarificador.Api.Services.DeepSeekService>();
builder.Services.AddScoped<Clarificador.Api.Services.TikTokScraperService>();
builder.Services.AddSingleton<Clarificador.Api.Services.ScrapingCoordinator>(); // Singleton, no Scoped


// 1. Obtener la cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("HostingerMySql");

// 2. Configurar el DbContext para usar MySQL (Pomelo)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
