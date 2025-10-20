using Microsoft.AspNetCore.Identity;

namespace censudex_clients_service.src.services
{
    /// <summary>
    /// Servicio de hashing de contraseñas utilizando algoritmo BCrypt.
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    public class BCryptService<TUser> : IPasswordHasher<TUser> where TUser : class
    {
        /// <summary>
        /// Genera un hash BCrypt de la contraseña dada.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string HashPassword(TUser user, string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        /// <summary>
        /// Verifica si la contraseña proporcionada coincide con el hash almacenado en la base de datos.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="hashedPassword"></param>
        /// <param name="providedPassword"></param>
        /// <returns></returns>
        public PasswordVerificationResult VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword)
        {
            bool isValid = BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
            return isValid ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
        }
    }
}