using CataVentoApi.Entity.Dto.RequestDto;
using CataVentoApi.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CataVentoApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PostLikerController : ControllerBase
    {
        private readonly IPostLikerService _postLikerService;

        public PostLikerController(IPostLikerService postLikerService)
        {
            _postLikerService = postLikerService;
        }

        [HttpGet("{postId}/likestatus")]
        public async Task<IActionResult> GetLikeStatus(long postId, [FromQuery] long usuarioId)
        {
            var hasUserLiked = await _postLikerService.HasUserLikedIconAsync(postId, usuarioId);

            if(!hasUserLiked) return NotFound();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLike([FromBody] PostLikerRequestDto request)
        {
            var toggleLike = await _postLikerService.ToggleLikeAsync(request);

            if (!toggleLike) return BadRequest();

            return Ok();
        }
    }
}
