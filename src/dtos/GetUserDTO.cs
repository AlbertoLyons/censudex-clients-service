using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace censudex_clients_service.src.dtos
{
    /// <summary>
    /// DTO para obtener la información de un usuario.
    /// </summary>
    public class GetUserDTO
    {
        /// <summary>
        /// Identificador único del usuario.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Nombre completo del usuario.
        /// </summary>
        public string FullName { get; set; } = null!;
        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        public string Email { get; set; } = null!;
        /// <summary>
        /// Nombre de usuario.
        /// </summary>
        public string UserName { get; set; } = null!;
        /// <summary>
        /// Estado del usuario (activo/inactivo).
        /// </summary>
        public bool Status { get; set; }
        /// <summary>
        /// Fecha de nacimiento del usuario.
        /// </summary>
        public DateOnly BirthDate { get; set; }
        /// <summary>
        /// Dirección del usuario.
        /// </summary>
        public string Address { get; set; } = null!;
        /// <summary>
        /// Número de teléfono del usuario.
        /// </summary>
        public string PhoneNumber { get; set; } = null!;
        /// <summary>
        /// Fecha de creación del usuario.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}