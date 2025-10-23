using censudex_clients_service.src.mappers;
using censudex_clients_service.src.models;
using censudex_clients_service.src.services;
using Grpc.Core;

using Microsoft.AspNetCore.Identity;

namespace CensudexUsersService.Services
{
    public class UserService : UserProto.UserService.UserServiceBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SendGridService _sendGridService;

        public UserService(UserManager<User> userManager, SendGridService sendGridService)
        {
            _userManager = userManager;
            _sendGridService = sendGridService;
        }
        public override Task<UserProto.CreateUserResponse> Create(UserProto.CreateUserRequest request, ServerCallContext context)
        {
            if (request == null)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "La solicitud no puede ser nula"));
            }
            if (_userManager.Users.Any(u => u.Email == request.Email))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "El correo electrónico ya está en uso"));
            }
            if (DateOnly.TryParse(request.Birthdate, out var birthDate))
            {
                var age = DateTime.UtcNow.Year - birthDate.Year;
                if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-age)) age--;
                if (age < 18)
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "El usuario debe ser mayor de 18 años"));
                }
            }
            var user = UserMapper.RegisterToUser(request);
            var result = _userManager.CreateAsync(user, request.Password).Result;

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
        public override Task<UserProto.GetAllResponse> GetAll(UserProto.GetAllRequest request, ServerCallContext context)
        {
            var response = new UserProto.GetAllResponse();
            var users = _userManager.Users.ToList();
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

            var usersProto = UserMapper.UsersToGetUserDTOs(users);
            response.Users.AddRange(usersProto);
            return Task.FromResult(response);
        }
        public override Task<UserProto.GetUserByIdResponse> GetById(UserProto.GetUserByIdRequest request, ServerCallContext context)
        {
            var response = new UserProto.GetUserByIdResponse();
            var user = _userManager.FindByIdAsync(request.Id).Result;
            if (user == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Usuario no encontrado"));
            }
            var getUser = UserMapper.UserToGetUserDTO(user);
            response.User = getUser;
            return Task.FromResult(response);
        }
        public override async Task<UserProto.UpdateUserResponse> Update(UserProto.UpdateUserRequest request, ServerCallContext context)
        {
            if (request == null)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "La solicitud no puede ser nula"));
            }
            var user = _userManager.FindByIdAsync(request.Id).Result;
            if (user == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Usuario no encontrado"));
            }
            if (_userManager.Users.Any(u => u.Email == request.Email && request.Email != user.Email))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "El correo electrónico ya está en uso"));
            }
            if (DateOnly.TryParse(request.Birthdate, out var birthDate))
            {
                var age = DateTime.UtcNow.Year - birthDate.Year;
                if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-age)) age--;
                if (age < 18)
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "El usuario debe ser mayor de 18 años"));
                }
            }

            var response = new UserProto.UpdateUserResponse();
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

            if (user == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Usuario no encontrado"));
            }
            user = UserMapper.EditUserDTOToUser(request, user);
            var result = await _userManager.UpdateAsync(user);
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
        public override Task<UserProto.DeleteUserResponse> Delete(UserProto.DeleteUserRequest request, ServerCallContext context)
        {
            var response = new UserProto.DeleteUserResponse();
            var user = _userManager.FindByIdAsync(request.Id).Result;
            if (user == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Usuario no encontrado"));
            }
            user.Status = false;
            user.DeletedAt = DateTime.UtcNow;
            var result = _userManager.UpdateAsync(user).Result;
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
        public override Task<UserProto.VerifyCredentialsResponse> VerifyCredentials(UserProto.VerifyCredentialsRequest request, ServerCallContext context)
        {
            var response = new UserProto.VerifyCredentialsResponse();
            var user = _userManager.FindByNameAsync(request.Username).Result
                ?? _userManager.FindByEmailAsync(request.Username).Result;
            if (user == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Cliente no encontrado"));
            }
            var passwordValid = _userManager.CheckPasswordAsync(user, request.Password).Result;
            if (!passwordValid)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Credenciales inválidas"));
            }
            response.Id = user.Id.ToString();
            response.Roles.AddRange(_userManager.GetRolesAsync(user).Result);
            return Task.FromResult(response);
        }
        public override async Task<UserProto.SendEmailResponse> SendEmail(UserProto.SendEmailRequest request, ServerCallContext context)
        {
            var response = new UserProto.SendEmailResponse();
            if (_userManager.Users.All(u => u.Email != request.Toemail))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "El correo electrónico del destinatario no está registrado"));
            }
            var emailSentResponse = await SendGridService.SendEmailAsync(
                request.Fromemail,
                request.Toemail,
                request.Subject,
                request.Plaintextcontent,
                request.Htmlcontent);
            Console.WriteLine(emailSentResponse.StatusCode);
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