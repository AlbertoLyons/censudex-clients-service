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
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            var user = UserMapper.RegisterToUser(registerDTO);

            var result = await _userManager.CreateAsync(user, registerDTO.Password);

            if (result.Succeeded)
            {
                return Ok(new { Message = "User registered successfully" });
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

    }
}