using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using censudex_clients_service.src.dtos;
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
        public static User RegisterToUser(RegisterDTO registerDTO)
        {
            return new User
            {
                // Combinación de nombres y apellidos en FullName
                FullName = $"{registerDTO.Names} {registerDTO.LastNames}",
                Email = registerDTO.Email,
                UserName = registerDTO.Username,
                // Estado inicial del usuario, por defecto activo
                Status = true,
                BirthDate = registerDTO.BirthDate,
                Address = registerDTO.Address,
                PhoneNumber = registerDTO.PhoneNumber,
                // Fecha de creación establecida a la fecha y hora actual UTC
                CreatedAt = DateTime.UtcNow
            };
        }
        /// <summary>
        /// Convierte una lista de entidades User a una lista de GetUserDTOs para el método GetAllUsers.
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        public static List<GetUserDTO> UsersToGetUserDTOs(List<User> users)
        {
            var getUserDTOs = new List<GetUserDTO>();
            foreach (var user in users)
            {
                getUserDTOs.Add(new GetUserDTO
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email!,
                    UserName = user.UserName!,
                    Status = user.Status,
                    BirthDate = user.BirthDate,
                    Address = user.Address,
                    PhoneNumber = user.PhoneNumber!,
                    CreatedAt = user.CreatedAt
                });
            }
            return getUserDTOs;
        }
        /// <summary>
        /// Convierte una entidad User a un GetUserDTO para el método GetUserById.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static GetUserDTO UserToGetUserDTO(User user)
        {
            return new GetUserDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                UserName = user.UserName!,
                Status = user.Status,
                BirthDate = user.BirthDate,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber!,
                CreatedAt = user.CreatedAt
            };
        }
        /// <summary>
        /// Actualiza una entidad User existente con los datos proporcionados en un EditUserDTO.
        /// </summary>
        /// <param name="editUserDTO"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static User EditUserDTOToUser(EditUserDTO editUserDTO, User user)
        {
            // Combinación de nombres y apellidos en FullName
            user.FullName = $"{editUserDTO.Names} {editUserDTO.LastNames}";
            user.Email = editUserDTO.Email;
            user.UserName = editUserDTO.Username;
            user.BirthDate = editUserDTO.BirthDate;
            user.Address = editUserDTO.Address;
            user.PhoneNumber = editUserDTO.PhoneNumber;
            return user;
        }
    }
}