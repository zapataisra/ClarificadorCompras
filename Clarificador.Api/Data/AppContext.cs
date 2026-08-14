using Microsoft.EntityFrameworkCore;

namespace Clarificador.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Aquí mapearemos nuestras tres tablas (Productos, Variaciones, Consultas) en el siguiente paso
    }
}