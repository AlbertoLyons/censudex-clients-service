using censudex_clients_service.src.dtos;
using censudex_clients_service.src.mappers;
using censudex_clients_service.src.models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace censudex_clients_service.src.controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        public UserController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            if (_userManager.Users.Any(u => u.Email == registerDTO.Email))
            {
                return BadRequest(new { Message = "El correo electrónico ya está en uso" });
            }
            if (registerDTO.BirthDate > DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-18)))
            {
                return BadRequest(new { Message = "El cliente debe ser mayor de 18 años" });
            }
            var user = UserMapper.RegisterToUser(registerDTO);

            var result = await _userManager.CreateAsync(user, registerDTO.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Client");
                return Ok(new { Message = "Cliente registrado exitosamente" });
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }
        [HttpGet("getAll")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? NameFilter = null,
            [FromQuery] string? EmailFilter = null,
            [FromQuery] bool? StatusFilter = null,
            [FromQuery] string? UsernameFilter = null
        )
        {
            var users = _userManager.Users.ToList();
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
            var usersDTO = UserMapper.UsersToGetUserDTOs(users);
            return Ok(usersDTO);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }
            var userDTO = UserMapper.UserToGetUserDTO(user);
            return Ok(userDTO);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(Guid id, [FromBody] EditUserDTO editUserDTO)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound("Cliente no encontrado");
            }
            if (_userManager.Users.Any(u => u.Email == editUserDTO.Email && editUserDTO.Email != user.Email))
            {
                return BadRequest(new { Message = "El correo electrónico ya está en uso" });
            }
            if (editUserDTO.BirthDate > DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-18)))
            {
                return BadRequest(new { Message = "La fecha de nacimiento debe ser mayor de 18 años" });
            }
            if (!string.IsNullOrEmpty(editUserDTO.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, editUserDTO.Password);
                if (!passwordResult.Succeeded)
                {
                    return BadRequest(passwordResult.Errors);
                }
            }
            user = UserMapper.EditUserDTOToUser(editUserDTO, user);
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Ok(new { Message = "Cliente actualizado exitosamente" });
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound("Cliente no encontrado");
            }
            user.Status = false;
            user.DeletedAt = DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Ok(new { Message = "Cliente eliminado exitosamente" });
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }
    }
}