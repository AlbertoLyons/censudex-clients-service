using censudex_clients_service.src.models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace censudex_clients_service.src.data
{
    /// <summary>
    /// DataContext para la aplicación, incluyendo la configuración de Identity. Actualmente utiliza la base de datos PostgreSQL.
    /// </summary>
    public class DataContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        /// <summary>
        /// Constructor del DataContext.
        /// </summary>
        /// <param name="options"></param>
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
        /// <summary>
        /// Configuración del modelo y datos iniciales, añadiendo roles predeterminados.
        /// </summary>
        /// <param name="builder"></param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Llamada al método base para asegurar la configuración predeterminada de Identity.
            base.OnModelCreating(builder);
            // Definición de roles predeterminados.
            List<IdentityRole<Guid>> roles = new List<IdentityRole<Guid>>
            {
                // Rol de administrador.
                new IdentityRole<Guid>
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                // Rol de cliente.
                new IdentityRole<Guid>
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Client",
                    NormalizedName = "CLIENT"
                }
            };
            // Inserción de los roles en la base de datos.
            builder.Entity<IdentityRole<Guid>>().HasData(roles);
        }
    }
}