using CataVentoApi.Entity.Dto.RequestDto;
using CataVentoApi.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CataVentoApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpGet("{postId}")]
        public async Task<IActionResult> GetPostById(long postId)
        {
            var post = await _postService.GetPostByIdAsync(postId);
            if (post == null)
            {
                return NotFound("Post not found.");
            }
            return Ok(post);
        }

        [HttpGet("search-by-content")]
        public async Task<IActionResult> GetPostByContent([FromQuery] string content)
        {
            var post = await _postService.GetPostByContentAsync(content);
            if (post == null)
            {
                return NotFound("Post not found.");
            }
            return Ok(post);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetPostsByUserId(
            long userId, 
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5)
        {
            var posts = await _postService.GetPostsByUserIdAsync(userId, pageNumber, pageSize);
            return Ok(posts);
        }

        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetPostsByGroupId(
            long groupId, 
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5)
        {
            var posts = await _postService.GetPostsByGroupIdAsync(groupId, pageNumber, pageSize);
            return Ok(posts);
        }

        [HttpGet("group/{groupId}/date")]
        public async Task<IActionResult> GetPostsByGroupIdAndDateAsync(long groupId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var posts = await _postService.GetPostsByGroupIdAndDateAsync(groupId, startDate, endDate);
            return Ok(posts);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] PostRequestDto postRequestDto)
        {
            var createdPost = await _postService.CreatePostAsync(postRequestDto);
            
            if(createdPost == null)
                return BadRequest("Invalid GroupId or CreatorId.");

            return CreatedAtAction(nameof(GetPostById), new { postId = createdPost.PostId }, createdPost);
        }

        [HttpPut("{postId}")]
        public async Task<IActionResult> UpdatePost(long postId, [FromBody] PostRequestDto postRequestDto)
        {
            var result = await _postService.UpdatePostAsync(postId, postRequestDto);
            if (!result)
            {
                return NotFound("Or Post or UserId or GroupId nor found");
            }
            return NoContent();
        }

        [HttpPatch("{postId}/image")]
        public async Task<IActionResult> PatchPostImageUrl(long postId, [FromBody] string imageUrl)
        {
            var result = await _postService.PatchPostImageUrlAsync(postId, imageUrl);
            if (!result)
            {
                return NotFound("Post not found.");
            }
            return NoContent();
        }

        [HttpDelete("{postId}")]
        public async Task<IActionResult> DeletePost(long postId)
        {
            var result = await _postService.DeletePostAsync(postId);
            if (!result)
            {
                return NotFound("Post not found.");
            }
            return NoContent();
        }
    }
}
