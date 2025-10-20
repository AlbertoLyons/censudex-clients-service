using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace censudex_clients_service.src.dtos
{
    /// <summary>
    /// DTO para la edición de un usuario existente.
    /// </summary>
    public class EditUserDTO
    {
        /// <summary>
        /// Nombres del usuario.
        /// </summary>
        public required string Names { get; set; }
        /// <summary>
        /// Apellidos del usuario.
        /// </summary>
        public required string LastNames { get; set; }
        /// <summary>
        /// Correo electrónico del usuario. Debe terminar en @censudex.cl
        /// </summary>
        [EmailAddress]
        [RegularExpression(@"^[^@\s]+@censudex\.cl$", ErrorMessage = "El correo debe de terminar en @censudex.cl")]
        public required string Email { get; set; }
        /// <summary>
        /// Nombre de usuario.
        /// </summary>
        public required string Username { get; set; }
        /// <summary>
        /// Fecha de nacimiento del usuario.
        /// </summary>
        public required DateOnly BirthDate { get; set; }
        /// <summary>
        /// Dirección del usuario.
        /// </summary>
        public required string Address { get; set; }
        /// <summary>
        /// Número de teléfono del usuario.
        /// </summary>
        [Phone]
        [RegularExpression(@"^\+569\d{8}$", ErrorMessage = "El número de teléfono debe ser válido y comenzar con el código +569")]
        public required string PhoneNumber { get; set; }
        /// <summary>
        /// Contraseña a cambiar del usuario. Esta puede ser opcional.
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }
}