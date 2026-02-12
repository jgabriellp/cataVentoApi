using CataVentoApi.Entity;
using CataVentoApi.Entity.Dto.RequestDto;
using CataVentoApi.Entity.Enums;
using CataVentoApi.Repositories.Interface;
using CataVentoApi.Services.Interface;
using System.Threading.Tasks;

namespace CataVentoApi.Services.Service
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IUsuarioRepository _usuarioRepository;

        public TaskService(ITaskRepository taskRepository, IUsuarioRepository usuarioRepository)
        {
            _taskRepository = taskRepository;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<int> CreateTask(KanbanTask task)
        {
            var user = await _usuarioRepository.GetById(task.UsuarioId ?? 0);

            if (user == null) return 0;

            var taskId = await _taskRepository.CreateTaskAsync(task);
            return taskId;
        }

        public async Task<KanbanTask?> GetTaskById(int id)
        { 
            var task = await _taskRepository.GetTaskByIdAsync(id);
            
            if (task == null) return null;

            return task;
        }

        public async Task<IEnumerable<KanbanTask>> GetAllTasks(KanbanBoardTypeEnum boardType)
            => await _taskRepository.GetAllTasksAsync(boardType);

        public async Task<bool> ReorderTasks(List<UpdatePriorityRequestDto> tasks)
            => await _taskRepository.ReorderTasksAsync(tasks);

        public async Task<KanbanTask?> UpdateTask(KanbanTask task)
        {
            var user = await _usuarioRepository.GetById(task.UsuarioId ?? 0);
            var existingTask = await _taskRepository.GetTaskByIdAsync(task.Id);

            if (user == null || existingTask == null) return null;

            var updated = await _taskRepository.UpdateTaskAsync(task);

            if (updated == false) return null;

            var updatedtask = new KanbanTask
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                UsuarioId = task.UsuarioId,
                DueDate = task.DueDate,
                Priority = task.Priority,
                Status = task.Status,
                Position = task.Position,
                Responsavel = new UsuarioResponsavel
                {
                    Id = user.Id,
                    Name = user.Name
                },
                BoardType = task.BoardType
            };

            return updatedtask;
        }

        public async Task<bool> UpdateColumn(int id, KanbanTaskStatusEnum newStatus, int newPosition)
        {
            var existingTask = await _taskRepository.GetTaskByIdAsync(id);
            if (existingTask == null) return false;
            return await _taskRepository.UpdateColumnAsync(id, newStatus, newPosition);
        }

        public async Task<bool> DeleteTask(int id)
        {
            var existingTask = await _taskRepository.GetTaskByIdAsync(id);

            if (existingTask == null) return false;

            return await _taskRepository.DeleteTaskAsync(id);
        }
    }
}
