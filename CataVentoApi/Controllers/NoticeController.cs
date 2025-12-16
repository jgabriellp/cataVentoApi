using CataVentoApi.Entity;
using CataVentoApi.Entity.Dto.RequestDto;
using CataVentoApi.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CataVentoApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NoticeController : ControllerBase
    {
        private readonly INoticeService _noticeService;

        public NoticeController(INoticeService noticeService)
        {
            _noticeService = noticeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllNotices(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5)
        {
            var notices = await _noticeService.GetAllNoticesAsync(pageNumber, pageSize);
            return Ok(notices);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetAllActiveNotices(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5)
        {
            var notices = await _noticeService.GetAllNoticesActiveAsync(pageNumber, pageSize);
            return Ok(notices);
        }

        [HttpGet("id/{id}")]
        public async Task<IActionResult> GetNoticeById(long id)
        {
            var notice = await _noticeService.GetNoticeByIdAsync(id);
            if (notice == null)
            {
                return NotFound();
            }
            return Ok(notice);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNotice([FromBody] NoticeResquestDto noticeResquestDto)
        {
            var noticeId = await _noticeService.AddNoticeAsync(noticeResquestDto);
            if(noticeId == 0) return BadRequest("Error creating notice.");
            return CreatedAtAction(nameof(GetNoticeById), new { id = noticeId }, noticeResquestDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNotice(long id, [FromBody] Notice notice)
        {
            if (id != notice.NoticeId)
            {
                return BadRequest("Notice ID mismatch.");
            }
            var result = await _noticeService.UpdateNoticeAsync(notice);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotice(long id)
        {
            var result = await _noticeService.DeleteNoticeAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
