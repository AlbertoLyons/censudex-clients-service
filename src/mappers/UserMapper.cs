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
                BirthDate = registerDTO.BirthDate,
                Address = registerDTO.Address,
                PhoneNumber = registerDTO.PhoneNumber,
            };
        }
    }
}