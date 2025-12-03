using CataVentoApi.Entity.Dto.RequestDto;
using CataVentoApi.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CataVentoApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5)
        {
            var users = await _usuarioService.GetAllUsersAsync(pageNumber, pageSize);
            return Ok(users);
        }

        [HttpGet("groupId/{groupId}")]
        public async Task<IActionResult> GetAllUsersByGroupId(long groupId)
        {
            var users = await _usuarioService.GetAllUsersByGroupIdAsync(groupId);
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(long id)
        {
            var user = await _usuarioService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var user = await _usuarioService.GetUserByEmailAsync(email);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [HttpGet("name/{name}")]
        public async Task<IActionResult> GetUserByName(string name)
        {
            var users = await _usuarioService.GetUserByNameAsync(name);
            if (users == null)
                return NotFound();
            return Ok(users);
        }


        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UsuarioRequestDto usuarioRequestDto)
        {
            var createdUser = await _usuarioService.CreateUserAsync(usuarioRequestDto);
            if (createdUser == null)
                return BadRequest("User with the same email already exists.");
            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
        }

        [HttpPut("update-password/{id}")]
        public async Task<IActionResult> UpdateUserPassword(long id, [FromBody] UpdatePasswordRequestDto updatePasswordRequestDto)
        {
            var result = await _usuarioService.SetNewPassword(id, updatePasswordRequestDto.oldPassword, updatePasswordRequestDto.newPassword);
            if (!result)
                return BadRequest("Old password is incorrect or user not found.");
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(long id, [FromBody] UsuarioRequestDto usuarioRequestDto)
        {
            var result = await _usuarioService.UpdateUserAsync(id, usuarioRequestDto);
            if (!result)
                return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(long id)
        {
            var result = await _usuarioService.DeleteUserAsync(id);
            if (!result)
                return NotFound();
            return NoContent();
        }
    }
}
