using censudex_clients_service.src.dtos;
using censudex_clients_service.src.mappers;
using censudex_clients_service.src.models;
using censudex_clients_service.src.services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace censudex_clients_service.src.controllers
{
    [ApiController]
    [Route("api/[controller]")]
    /// <summary>
    /// Controlador de ruta para la gestión de usuarios.
    /// </summary>
    public class UserController : ControllerBase
    {
        /// <summary>
        /// UserManager para la gestión de usuarios.
        /// </summary>
        private readonly UserManager<User> _userManager;
        /// <summary>
        /// Servicio de SendGrid para el envío de correos electrónicos.
        /// </summary>
        private readonly SendGridService _sendGridService;
        /// <summary>
        /// Constructor del controlador de usuarios.
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="sendGridService"></param>
        public UserController(UserManager<User> userManager, SendGridService sendGridService)
        {
            _userManager = userManager;
            _sendGridService = sendGridService;
        }
        /// <summary>
        /// Envía un correo electrónico utilizando SendGrid.
        /// </summary>
        /// <param name="sendMailDTO"></param>
        /// <returns></returns>
        [HttpPost("sendMail")]
        public async Task<IActionResult> SendMail([FromBody] SendMailDTO sendMailDTO)
        {
            // Verificar si el correo electrónico del destinatario está registrado en la base de datos.
            if (_userManager.Users.All(u => u.Email != sendMailDTO.ToEmail))
            {
                return BadRequest(new { Message = "El correo electrónico del destinatario no está registrado" });
            }
            // Enviar el correo electrónico utilizando el servicio de SendGrid.
            var response = await SendGridService.SendEmailAsync(
                sendMailDTO.FromEmail,
                sendMailDTO.ToEmail,
                sendMailDTO.Subject,
                sendMailDTO.PlainTextContent,
                sendMailDTO.HtmlContent
            );
            // Devolver el resultado del envío.
            if (response.StatusCode == System.Net.HttpStatusCode.Accepted || response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(new { Message = "Correo enviado exitosamente" });
            }
            // Devolver error en caso de fallo.
            else
            {
                return StatusCode((int)response.StatusCode, new { Message = "Error al enviar el correo" });
            }
        }
        /// <summary>
        /// Registra un nuevo usuario cliente.
        /// </summary>
        /// <param name="registerDTO"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            // Verificar si el correo electrónico ya está en uso.
            if (_userManager.Users.Any(u => u.Email == registerDTO.Email))
            {
                return BadRequest(new { Message = "El correo electrónico ya está en uso" });
            }
            // Verificar si el usuario es mayor de 18 años.
            if (registerDTO.BirthDate > DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-18)))
            {
                return BadRequest(new { Message = "El cliente debe ser mayor de 18 años" });
            }
            // Mapear RegisterDTO a User.
            var user = UserMapper.RegisterToUser(registerDTO);
            // Crear el usuario.
            var result = await _userManager.CreateAsync(user, registerDTO.Password);
            // Asignar el rol de "Client" al usuario en caso de éxito.
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Client");
                return Ok(new { Message = "Cliente registrado exitosamente" });
            }
            // Devolver errores en caso de fallo.
            else
            {
                return BadRequest(result.Errors);
            }
        }
        /// <summary>
        /// Obtiene todos los usuarios con filtros opcionales.
        /// </summary>
        /// <param name="NameFilter"></param>
        /// <param name="EmailFilter"></param>
        /// <param name="StatusFilter"></param>
        /// <param name="UsernameFilter"></param>
        /// <returns></returns>
        [HttpGet("getAll")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? NameFilter = null,
            [FromQuery] string? EmailFilter = null,
            [FromQuery] bool? StatusFilter = null,
            [FromQuery] string? UsernameFilter = null
        )
        {
            // Obtener todos los usuarios
            var users = _userManager.Users.ToList();
            // Aplicar filtros si se proporcionan
            if (!string.IsNullOrEmpty(NameFilter))
            {
                users = users.Where(u => u.FullName.Contains(NameFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (!string.IsNullOrEmpty(EmailFilter))
            {
                users = users.Where(u => u.Email!.Contains(EmailFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (StatusFilter.HasValue)
            {
                users = users.Where(u => u.Status == StatusFilter.Value).ToList();
            }
            if (!string.IsNullOrEmpty(UsernameFilter))
            {
                users = users.Where(u => u.UserName!.Contains(UsernameFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            // Mapear a DTOs y devolver
            var usersDTO = UserMapper.UsersToGetUserDTOs(users);
            return Ok(usersDTO);
        }
        /// <summary>
        /// Obtiene un usuario por su ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            // Buscar el usuario por ID
            var user = await _userManager.FindByIdAsync(id.ToString());
            // Si no se encuentra, devolver NotFound
            if (user == null)
            {
                return NotFound();
            }
            // Mapear a DTO y devolver
            var userDTO = UserMapper.UserToGetUserDTO(user);
            return Ok(userDTO);
        }
        /// <summary>
        /// Edita un usuario existente dado su ID y los nuevos datos.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="editUserDTO"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(Guid id, [FromBody] EditUserDTO editUserDTO)
        {
            // Buscar el usuario por ID
            var user = await _userManager.FindByIdAsync(id.ToString());
            // Si no se encuentra, devolver NotFound
            if (user == null)
            {
                return NotFound("Cliente no encontrado");
            }
            // Validar correo electrónico único pero excluyendo el del usuario actual
            if (_userManager.Users.Any(u => u.Email == editUserDTO.Email && editUserDTO.Email != user.Email))
            {
                return BadRequest(new { Message = "El correo electrónico ya está en uso" });
            }
            // Validar que la fecha de nacimiento sea mayor de 18 años
            if (editUserDTO.BirthDate > DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-18)))
            {
                return BadRequest(new { Message = "La fecha de nacimiento debe ser mayor de 18 años" });
            }
            /* 
                Si se proporciona una nueva contraseña, actualizarla. 
                Este paso debe hacerse antes de mapear otros datos en caso de que se haya proporcionado una nueva contraseña.
            */
            if (!string.IsNullOrEmpty(editUserDTO.Password))
            {
                // Validar que la nueva contraseña cumpla con los requisitos
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                // Restablecer la contraseña
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, editUserDTO.Password);
                // Si falla, devolver errores
                if (!passwordResult.Succeeded)
                {
                    return BadRequest(passwordResult.Errors);
                }
            }
            // Mapear los nuevos datos al usuario existente
            user = UserMapper.EditUserDTOToUser(editUserDTO, user);
            // Actualizar el usuario
            var result = await _userManager.UpdateAsync(user);
            // Devolver resultado
            if (result.Succeeded)
            {
                return Ok(new { Message = "Cliente actualizado exitosamente" });
            }
            // Si falla, devolver errores
            else
            {
                return BadRequest(result.Errors);
            }
        }
        /// <summary>
        /// Elimina (desactiva con Soft Delete) un usuario dado su ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            // Buscar el usuario por ID
            var user = await _userManager.FindByIdAsync(id.ToString());
            // Si no se encuentra, devolver NotFound
            if (user == null)
            {
                return NotFound("Cliente no encontrado");
            }
            // Realizar Soft Delete, desactivando el usuario y estableciendo DeletedAt
            user.Status = false;
            user.DeletedAt = DateTime.UtcNow;
            // Actualizar el usuario
            var result = await _userManager.UpdateAsync(user);
            // Devolver resultado
            if (result.Succeeded)
            {
                return Ok(new { Message = "Cliente eliminado exitosamente" });
            }
            // Si falla, devolver errores
            else
            {
                return BadRequest(result.Errors);
            }
        }
        /// <summary>
        /// Verifica las credenciales de un usuario (nombre de usuario y contraseña).
        /// Retorna el rol del usuario si las credenciales son válidas.
        /// </summary>
        /// <param name="verifyCredentialsDTO"></param>
        /// <returns></returns>
        [HttpPost("verifyCredentials")]
        public async Task<IActionResult> VerifyCredentials([FromBody] VerifyCredentialsDTO verifyCredentialsDTO)
        {
            // Buscar el usuario por nombre de usuario o correo electrónico
            var user = await _userManager.FindByNameAsync(verifyCredentialsDTO.Username)
                ?? await _userManager.FindByEmailAsync(verifyCredentialsDTO.Username);
            // Si no se encuentra, devolver NotFound
            if (user == null)
            {
                return NotFound(new { Message = "Usuario no encontrado" });
            }
            // Verificar la contraseña
            var passwordValid = await _userManager.CheckPasswordAsync(user, verifyCredentialsDTO.Password);
            if (!passwordValid)
            {
                return BadRequest(new { Message = "Credenciales inválidas" });
            }
            // Devolver éxito si las credenciales son válidas
            var response = new
            {
                Message = "Credenciales válidas",
                Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault()
            };
            return Ok(response);
        }
    }
}