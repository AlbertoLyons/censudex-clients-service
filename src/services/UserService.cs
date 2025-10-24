using censudex_clients_service.src.mappers;
using censudex_clients_service.src.models;
using censudex_clients_service.src.services;
using Grpc.Core;

using Microsoft.AspNetCore.Identity;

namespace CensudexUsersService.Services
{
    /// <summary>
    /// Servicio gRPC para la gestión de clientes. Utiliza UserManager de Identity para operaciones CRUD y autenticación.
    /// También utiliza el servicio SendGridService para el envío de correos electrónicos.
    /// </summary>
    public class UserService : UserProto.UserService.UserServiceBase
    {
        /// <summary>
        /// Gestor de usuarios de Identity.
        /// </summary>
        private readonly UserManager<User> _userManager;
        /// <summary>
        /// Inicializa una nueva instancia del servicio UserService.
        /// </summary>
        /// <param name="userManager"></param>
        public UserService(UserManager<User> userManager)
        {
            // Inyección de dependencia del UserManager
            _userManager = userManager;
        }
        /// <summary>
        /// Crea un nuevo usuario basado en la información proporcionada en CreateUserRequest de gRPC.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns>Mensaje indicando el estado de la creación del usuario</returns>
        /// <exception cref="RpcException"></exception>
        public override Task<UserProto.CreateUserResponse> Create(UserProto.CreateUserRequest request, ServerCallContext context)
        {
            // Validaciones en caso de errores
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "La solicitud no puede ser nula"));
            }
            // Verificar que el correo electrónico termine en @censudex.cl
            if (!request.Email.EndsWith("@censudex.cl"))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "El correo electrónico debe terminar en censudex.cl"));
            }
            // Verificar si el correo electrónico ya está en uso
            if (_userManager.Users.Any(u => u.Email == request.Email)) throw new RpcException(new Status(StatusCode.InvalidArgument, "El correo electrónico ya está en uso"));
            // Validar formato de fecha de nacimiento
            try
            {
                var age = DateOnly.Parse(request.Birthdate);
            }
            // Catch para formato inválido
            catch (Exception)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "La fecha de nacimiento no es válida. Formato esperado: AAAA-MM-DD"));
            }
            // Verificar si el usuario es mayor de edad
            if (DateOnly.TryParse(request.Birthdate, out var birthDate))
            {
                var age = DateTime.UtcNow.Year - birthDate.Year;
                if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-age)) age--;
                if (age < 18) throw new RpcException(new Status(StatusCode.InvalidArgument, "El usuario debe ser mayor de 18 años"));
            }
            // Mapeo del DTO a entidad User
            var user = UserMapper.RegisterToUser(request);
            // Creación del usuario en la base de datos
            var result = _userManager.CreateAsync(user, request.Password).Result;
            // Retorno de la respuesta o excepción en caso de error
            if (result.Succeeded)
            {
                return Task.FromResult(new UserProto.CreateUserResponse
                {
                    Message = "Cliente creado exitosamente"
                });
            }
            else
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Error al crear el usuario: " +
                    string.Join("; ", result.Errors.Select(e => e.Description))));
            }
        }
        /// <summary>
        /// Obtiene todos los usuarios, con filtros opcionales por nombre, correo electrónico, estado y nombre de usuario.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns>Lista de usuarios</returns>
        /// <exception cref="RpcException"></exception>
        public override Task<UserProto.GetAllResponse> GetAll(UserProto.GetAllRequest request, ServerCallContext context)
        {
            // Inicialización de la respuesta
            var response = new UserProto.GetAllResponse();
            // Obtención de todos los usuarios
            var users = _userManager.Users.ToList();
            // Aplicación de filtros si se proporcionan
            if (!string.IsNullOrEmpty(request.Namefilter))
            {
                users = users.Where(u => u.FullName.Contains(request.Namefilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (!string.IsNullOrEmpty(request.Emailfilter))
            {
                users = users.Where(u => u.Email!.Contains(request.Emailfilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (!string.IsNullOrEmpty(request.Statusfilter))
            {
                // Validar que el filtro de estado sea un booleano válido
                if (!bool.TryParse(request.Statusfilter, out bool statusValue))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "El filtro de estado no es válido"));
                }
                users = users.Where(u => u.Status == statusValue).ToList();
            }
            if (!string.IsNullOrEmpty(request.Usernamefilter))
            {
                users = users.Where(u => u.UserName!.Contains(request.Usernamefilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            // Mapeo de entidades User a DTOs y adición a la respuesta
            var usersProto = UserMapper.UsersToGetUserDTOs(users);
            // Retorno de la respuesta con la lista de usuarios
            response.Users.AddRange(usersProto);
            return Task.FromResult(response);
        }
        /// <summary>
        /// Obtiene un usuario por su ID.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns>Usuario</returns>
        /// <exception cref="RpcException"></exception>
        public override Task<UserProto.GetUserByIdResponse> GetById(UserProto.GetUserByIdRequest request, ServerCallContext context)
        {
            // Inicialización de la respuesta
            var response = new UserProto.GetUserByIdResponse();
            // Búsqueda del usuario por ID
            var user = _userManager.FindByIdAsync(request.Id).Result;
            // Si no se encuentra, lanzar excepción
            if (user == null) throw new RpcException(new Status(StatusCode.NotFound, "Usuario no encontrado"));
            // Mapeo de la entidad User a DTO
            var getUser = UserMapper.UserToGetUserDTO(user);
            // Asignación del DTO a la respuesta
            response.User = getUser;
            // Retorno de la respuesta
            return Task.FromResult(response);
        }
        /// <summary>
        /// Actualiza un usuario existente con la información proporcionada en UpdateUserRequest de gRPC.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns>Mensaje de respuesta</returns>
        /// <exception cref="RpcException"></exception>
        public override async Task<UserProto.UpdateUserResponse> Update(UserProto.UpdateUserRequest request, ServerCallContext context)
        {
            // Validaciones en caso de errores
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Id))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "La solicitud no puede ser nula. Unicamente la contraseña es opcional"));
            }
            if (!request.Email.EndsWith("@censudex.cl"))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "El correo electrónico debe terminar en censudex.cl"));
            }
            // Validar formato de fecha de nacimiento
            try
            {
                var age = DateOnly.Parse(request.Birthdate);
            }
            // Catch para formato inválido
            catch (Exception)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "La fecha de nacimiento no es válida. Formato esperado: AAAA-MM-DD"));
            }
            // Búsqueda del usuario por ID
            var user = _userManager.FindByIdAsync(request.Id).Result;
            // Si no se encuentra, lanzar excepción
            if (user == null) throw new RpcException(new Status(StatusCode.NotFound, "Usuario no encontrado"));
            // Verificar si el correo electrónico ya está en uso por otro usuario
            if (_userManager.Users.Any(u => u.Email == request.Email && request.Email != user.Email)) throw new RpcException(new Status(StatusCode.InvalidArgument, "El correo electrónico ya está en uso"));
            // Verificar si el usuario es mayor de edad
            if (DateOnly.TryParse(request.Birthdate, out var birthDate))
            {
                var age = DateTime.UtcNow.Year - birthDate.Year;
                if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-age)) age--;
                if (age < 18)
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "El usuario debe ser mayor de 18 años"));
                }
            }
            // Inicialización de la respuesta
            var response = new UserProto.UpdateUserResponse();
            // En caso de que se proporciona una nueva contraseña
            if (!string.IsNullOrEmpty(request.Password))
            {
                // Validar que la nueva contraseña cumpla con los requisitos
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                // Restablecer la contraseña
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, request.Password);
                // Si falla, devolver errores
                if (!passwordResult.Succeeded)
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Error al actualizar la contraseña: " +
                        string.Join("; ", passwordResult.Errors.Select(e => e.Description))));
                }
            }
            // Actualización de los datos del usuario            
            user = UserMapper.EditUserDTOToUser(request, user);
            // Guardar los cambios en la base de datos
            var result = await _userManager.UpdateAsync(user);
            // Retorno de la respuesta o excepción en caso de error
            if (result.Succeeded)
            {
                response.Message = "Usuario actualizado exitosamente";
                return await Task.FromResult(response);
            }
            else
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Error al actualizar el usuario: " +
                    string.Join("; ", result.Errors.Select(e => e.Description))));
            }
        }
        /// <summary>
        /// Elimina un usuario estableciendo su estado a inactivo (soft delete).
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns>Mensaje de respuesta</returns>
        /// <exception cref="RpcException"></exception>
        public override Task<UserProto.DeleteUserResponse> Delete(UserProto.DeleteUserRequest request, ServerCallContext context)
        {
            // Inicialización de la respuesta
            var response = new UserProto.DeleteUserResponse();
            // Búsqueda del usuario por ID
            var user = _userManager.FindByIdAsync(request.Id).Result;
            // Si no se encuentra, lanzar excepción
            if (user == null) throw new RpcException(new Status(StatusCode.NotFound, "Usuario no encontrado"));
            // Establecer el estado del usuario a inactivo y la fecha de eliminación
            user.Status = false;
            user.DeletedAt = DateTime.UtcNow;
            // Guardar los cambios en la base de datos
            var result = _userManager.UpdateAsync(user).Result;
            // Retorno de la respuesta o excepción en caso de error
            if (result.Succeeded)
            {
                response.Message = "Usuario eliminado exitosamente";
                return Task.FromResult(response);
            }
            else
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Error al eliminar el usuario: " +
                    string.Join("; ", result.Errors.Select(e => e.Description))));
            }
        }
        /// <summary>
        /// Verifica las credenciales de un usuario (nombre de usuario/correo electrónico y contraseña).
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns>ID de usuario y roles</returns>
        /// <exception cref="RpcException"></exception>
        public override Task<UserProto.VerifyCredentialsResponse> VerifyCredentials(UserProto.VerifyCredentialsRequest request, ServerCallContext context)
        {
            // Inicialización de la respuesta
            var response = new UserProto.VerifyCredentialsResponse();
            // Búsqueda del usuario por nombre de usuario o correo electrónico
            var user = _userManager.FindByNameAsync(request.Username).Result
                ?? _userManager.FindByEmailAsync(request.Username).Result;
            // Si no se encuentra, lanzar excepción
            if (user == null) throw new RpcException(new Status(StatusCode.NotFound, "Cliente no encontrado"));
            // Verificación de la contraseña
            var passwordValid = _userManager.CheckPasswordAsync(user, request.Password).Result;
            // Si la contraseña es inválida, lanzar excepción
            if (!passwordValid) throw new RpcException(new Status(StatusCode.InvalidArgument, "Credenciales inválidas"));
            // Si las credenciales son válidas, retornar el ID del usuario y sus roles
            response.Id = user.Id.ToString();
            // Obtener roles del usuario
            response.Roles.AddRange(_userManager.GetRolesAsync(user).Result);
            // Retorno de la respuesta
            return Task.FromResult(response);
        }
        /// <summary>
        /// Envía un correo electrónico utilizando el servicio SendGridService,
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns>Mensaje de respuesta</returns>
        /// <exception cref="RpcException"></exception>
        public override async Task<UserProto.SendEmailResponse> SendEmail(UserProto.SendEmailRequest request, ServerCallContext context)
        {
            // Inicialización de la respuesta
            var response = new UserProto.SendEmailResponse();
            // Validaciones en caso de que el correo electrónico del destinatario no esté registrados en la base de datos
            if (_userManager.Users.All(u => u.Email != request.Toemail)) throw new RpcException(new Status(StatusCode.InvalidArgument, "El correo electrónico del destinatario no está registrado"));
            // Envío del correo electrónico utilizando el servicio SendGridService
            var emailSentResponse = await SendGridService.SendEmailAsync(
                request.Fromemail,
                request.Toemail,
                request.Subject,
                request.Plaintextcontent,
                request.Htmlcontent);
            // Retorno de la respuesta o excepción en caso de error
            if (emailSentResponse.StatusCode == System.Net.HttpStatusCode.Accepted || emailSentResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                response.Message = "Correo electrónico enviado exitosamente";
                return await Task.FromResult(response);
            }
            else
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error al enviar el correo electrónico"));
            }
        }
    }
}