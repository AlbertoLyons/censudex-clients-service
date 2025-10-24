using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using censudex_clients_service.src.models;

namespace censudex_clients_service.src.mappers
{
    /// <summary>
    /// Mapeador estático para convertir entre User y sus DTOs asociados.
    /// </summary>
    public static class UserMapper
    {
        /// <summary>
        /// Convierte un RegisterDTO a una entidad User. Se utiliza para el registro de nuevos usuarios.
        /// </summary>
        /// <param name="registerDTO"></param>
        /// <returns></returns>
        public static User RegisterToUser(UserProto.CreateUserRequest createUserRequest)
        {
            return new User
            {
                // Combinación de nombres y apellidos en FullName
                FullName = $"{createUserRequest.Names} {createUserRequest.Lastnames}",
                Email = createUserRequest.Email,
                UserName = createUserRequest.Username,
                // Estado inicial del usuario, por defecto activo
                Status = true,
                BirthDate = DateOnly.Parse(createUserRequest.Birthdate),
                Address = createUserRequest.Address,
                PhoneNumber = createUserRequest.Phonenumber,
                // Fecha de creación establecida a la fecha y hora actual UTC
                CreatedAt = DateTime.UtcNow
            };
        }
        /// <summary>
        /// Convierte una lista de entidades User a una lista de GetUserDTOs para el método GetAllUsers.
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        public static List<UserProto.User> UsersToGetUserDTOs(List<User> users)
        {
            var getUserProto = new List<UserProto.User>();
            foreach (var user in users)
            {
                getUserProto.Add(new UserProto.User
                {
                    Id = user.Id.ToString(),
                    FullName = user.FullName,
                    Email = user.Email!,
                    UserName = user.UserName!,
                    Status = user.Status,
                    BirthDate = user.BirthDate.ToString("o"),
                    Address = user.Address,
                    Phonenumber = user.PhoneNumber!,
                    CreatedAt = user.CreatedAt.ToString("o")
                });
            }
            return getUserProto;
        }
        /// <summary>
        /// Convierte una entidad User a un GetUserDTO para el método GetUserById.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static UserProto.User UserToGetUserDTO(User user)
        {
            return new UserProto.User
            {
                Id = user.Id.ToString(),
                FullName = user.FullName,
                Email = user.Email!,
                UserName = user.UserName!,
                Status = user.Status,
                BirthDate = user.BirthDate.ToString("o"),
                Address = user.Address,
                Phonenumber = user.PhoneNumber!,
                CreatedAt = user.CreatedAt.ToString("o")
            };
        }
        /// <summary>
        /// Actualiza una entidad User existente con los datos proporcionados en un EditUserDTO.
        /// </summary>
        /// <param name="editUserDTO"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static User EditUserDTOToUser(UserProto.UpdateUserRequest editUser, User user)
        {
            // Combinación de nombres y apellidos en FullName
            user.FullName = $"{editUser.Names} {editUser.Lastnames}";
            user.Email = editUser.Email;
            user.UserName = editUser.Username;
            user.BirthDate = DateOnly.Parse(editUser.Birthdate);
            user.Address = editUser.Address;
            user.PhoneNumber = editUser.Phonenumber;
            return user;
        }
    }
}