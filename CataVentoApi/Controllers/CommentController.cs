using CataVentoApi.Entity.Dto.RequestDto;
using CataVentoApi.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CataVentoApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet("{commentId}")]
        public async Task<IActionResult> GetCommentById(long commentId)
        {
            var comment = await _commentService.GetCommentByIdAsync(commentId);
            
            if (comment == null)
                return NotFound("This comment doesn't exist.");
            
            return Ok(comment);
        }

        [HttpGet("postId/{postId}")]
        public async Task<IActionResult> GetCommentsByPostId(long postId)
        {
            var comments = await _commentService.GetCommentsByPostIdAsync(postId);
            return Ok(comments);
        }

        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] CommentRequestDto commentRequestDto)
        {
            var createComment = await _commentService.CreateCommentAsync(commentRequestDto);

            if (createComment == null)
                return NotFound("Or this post or user doesn't exist");
            
            return Ok(createComment);
        }

        [HttpPut("{commentId}")]
        public async Task<IActionResult> UpdateComment(long commentId, [FromBody] CommentRequestDto commentRequestDto)
        {
            var result = await _commentService.UpdateCommentAsync(commentId, commentRequestDto);

            if (!result)
                return NotFound("Or this comment or post or user doesn't exist");

            return NoContent();
        }

        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(long commentId)
        {
            var result = await _commentService.DeleteCommentAsync(commentId);

            if (!result)
                return NotFound("comment not found.");

            return NoContent();
        }
    }
}
