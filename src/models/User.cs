using Microsoft.AspNetCore.Identity;

namespace censudex_clients_service.src.models
{
    public class User : IdentityUser<Guid>
    {
        // Utiliza UUID V4
        public override Guid Id { get; set; } = Guid.NewGuid();
        // Nombre completo del usuario
        public string FullName { get; set; } = string.Empty;
        // Estado del usuario (activo/inactivo)
        public bool Status { get; set; } 
        // Fecha de nacimiento del usuario
        public DateOnly BirthDate { get; set; }
        // Dirección del usuario
        public string Address { get; set; } = string.Empty;
        // Fecha de registro del usuario
        public DateTime CreatedAt { get; set; }
    }
}