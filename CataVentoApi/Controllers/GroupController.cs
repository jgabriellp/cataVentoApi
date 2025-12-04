using CataVentoApi.Entity.Dto.RequestDto;
using CataVentoApi.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CataVentoApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GroupController : ControllerBase
    {
        private readonly IGroupService _groupService;

        public GroupController(IGroupService groupService)
        {
            _groupService = groupService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllGroups(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5)
        {
            var groups = await _groupService.GetAllGroupsAsync(pageNumber, pageSize);
            return Ok(groups);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGroupById(long id)
        {
            var group = await _groupService.GetGroupByIdAsync(id);
            if (group == null)
            {
                return NotFound();
            }
            return Ok(group);
        }

        [HttpGet("users/{userId}/groups")]
        public async Task<IActionResult> GetGroupsByUserId(long userId)
        {
            var groups = await _groupService.GetGroupsByUserIdAsync(userId);
            return Ok(groups);
        }

        [HttpGet("name/{groupName}")]
        public async Task<IActionResult> GetGroupByName(string groupName)
        {
            var group = await _groupService.GetGroupByNameAsync(groupName);
            if (group == null)
            {
                return NotFound();
            }
            return Ok(group);
        }

        [HttpPost("{groupId}/add-users")]
        public async Task<IActionResult> AddUsersToGroup(long groupId, [FromBody] List<long> userIds)
        {
            var group = await _groupService.AddUsersToGroupAsync(groupId, userIds);
            if (group == null)
            {
                return NotFound("User or Group not found.");
            }
            return Ok(group);
        }

        [HttpPost("{groupId}/remove-users")]
        public async Task<IActionResult> RemoveUsersFromGroup(long groupId, [FromBody] List<long> userIds)
        {
            var group = await _groupService.RemoveUsersFromGroupAsync(groupId, userIds);
            if (group == null)
            {
                return NotFound("Or this group was not found or one of them users doesn't exist.");
            }
            return Ok(group);
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromBody] GroupRequestDto groupRequestDto)
        {
            var group = await _groupService.CreateGroupAsync(groupRequestDto);
            if (group == null)
            {
                return BadRequest("Group could not be created.");
            }
            return CreatedAtAction(nameof(GetGroupById), new { id = group.GroupId }, group);
        }

        [HttpPut("{groupId}")]
        public async Task<IActionResult> UpdateGroup(int groupId, [FromBody] GroupUpdateRequestDto groupUpdateRequestDto)
        {
            var result = await _groupService.UpdateGroupAsync(groupId, groupUpdateRequestDto);
            if (!result)
            {
                return NotFound("Group not found.");
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroup(long id)
        {
            var result = await _groupService.DeleteGroupAsync(id);
            if (!result)
            {
                return NotFound("Group not found.");
            }
            return NoContent();
        }
    }
}
