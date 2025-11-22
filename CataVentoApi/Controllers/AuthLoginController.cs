using CataVentoApi.Entity.Dto.RequestDto;
using CataVentoApi.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CataVentoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthLoginController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public AuthLoginController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginRequestDto loginRequestDto)
        {
            var userAccess = _usuarioService.Login(loginRequestDto);
            if (userAccess == null || userAccess.Token == string.Empty)
            {
                return BadRequest(new { message = "UserEmail or Password is incorrect" });
            }
            return Ok(userAccess);
        }
    }
}
