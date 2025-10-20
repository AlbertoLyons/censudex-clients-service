
namespace censudex_clients_service.src.dtos
{
    /// <summary>
    /// DTO para verificar las credenciales de un usuario.
    /// </summary>
    public class VerifyCredentialsDTO
    {
        // Nombre de usuario del usuario. Puede ser el correo electrónico o el nombre de usuario.
        public required string Username { get; set; }
        // Contraseña del usuario.
        public required string Password { get; set; }
    }
}