using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            var user = UserMapper.RegisterToUser(registerDTO);

            var result = await _userManager.CreateAsync(user, registerDTO.Password);

            if (result.Succeeded)
            {
                return Ok(new { Message = "Cliente registrado exitosamente" });
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }
        [HttpGet("getAll")]
        public async Task<IActionResult> GetAll()
        {
            var users = _userManager.Users.ToList();
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