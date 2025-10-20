using Microsoft.AspNetCore.Identity;

namespace censudex_clients_service.src.models
{
    public class User : IdentityUser<Guid>
    {
        /// <summary>
        /// Identificador único del usuario (UUID V4).
        /// </summary>
        public override Guid Id { get; set; } = Guid.NewGuid();
        /// <summary>
        /// Nombre completo del usuario.
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        /// <summary>
        /// Estado del usuario (activo/inactivo para cumplir con Soft Delete).
        /// </summary>
        public bool Status { get; set; }
        /// <summary>
        /// Fecha de nacimiento del usuario.
        /// </summary>
        public DateOnly BirthDate { get; set; }
        /// <summary>
        /// Dirección del usuario.
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// Fecha de registro del usuario.
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// Fecha de eliminación del usuario (Soft delete).
        /// </summary>
        public DateTime DeletedAt { get; set; }
    }
}