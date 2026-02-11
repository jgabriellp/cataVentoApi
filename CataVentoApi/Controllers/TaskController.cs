using CataVentoApi.Entity;
using CataVentoApi.Entity.Dto.RequestDto;
using CataVentoApi.Entity.Enums;
using CataVentoApi.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CataVentoApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTasks([FromQuery] KanbanBoardTypeEnum boardType)
        {
            var tasks = await _taskService.GetAllTasks(boardType);
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var task = await _taskService.GetTaskById(id);

            if (task == null) return NotFound();

            return Ok(task);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] KanbanTask task)
        {
            var taskId = await _taskService.CreateTask(task);
            if (taskId == 0) return BadRequest("Failed to create task. User may not exist.");
            task.Id = taskId;
            return CreatedAtAction(nameof(GetTaskById), new { id = taskId }, task);
        }

        [HttpPost("reorder")]
        public async Task<IActionResult> ReorderTasks([FromBody] List<UpdatePriorityRequestDto> tasks)
        {
            var result = await _taskService.ReorderTasks(tasks);
            return result ? NoContent() : BadRequest("Erro ao reordenar tarefas.");
            //if (!result) return BadRequest("Failed to reorder tasks.");
            //return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] KanbanTask task)
        {
            if (id != task.Id) return BadRequest("Task ID mismatch.");
            var result = await _taskService.UpdateTask(task);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpPut("column")]
        public async Task<IActionResult> UpdateColumn([FromQuery] int id, [FromQuery] int newStatus, [FromQuery] int newPosition)
        {
            var result = await _taskService.UpdateColumn(id, (KanbanTaskStatusEnum)newStatus, newPosition);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var result = await _taskService.DeleteTask(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
