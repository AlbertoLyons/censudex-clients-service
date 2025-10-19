using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace censudex_clients_service.src.dtos
{
    public class EditUserDTO
    {
        public required string Names { get; set; }
        public required string LastNames { get; set; }
        [EmailAddress]
        [RegularExpression(@"^[^@\s]+@censudex\.cl$", ErrorMessage = "El correo debe de terminar en @censudex.cl")]
        public required string Email { get; set; }
        public required string Username { get; set; }
        public required DateOnly BirthDate { get; set; }
        public required string Address { get; set; }
        [Phone]
        public required string PhoneNumber { get; set; }
        public string Password { get; set; } = string.Empty;
    }
}