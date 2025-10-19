using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using censudex_clients_service.src.dtos;
using censudex_clients_service.src.models;

namespace censudex_clients_service.src.mappers
{
    public static class UserMapper
    {
        public static User RegisterToUser(RegisterDTO registerDTO)
        {
            return new User
            {
                FullName = $"{registerDTO.Names} {registerDTO.LastNames}",
                Email = registerDTO.Email,
                UserName = registerDTO.Username,
                Status = true,
                BirthDate = registerDTO.BirthDate,
                Address = registerDTO.Address,
                PhoneNumber = registerDTO.PhoneNumber,
                CreatedAt = DateTime.UtcNow
            };
        }
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
        public static User EditUserDTOToUser(EditUserDTO editUserDTO, User user)
        {
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