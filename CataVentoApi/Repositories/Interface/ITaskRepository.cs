using CataVentoApi.Entity;
using CataVentoApi.Entity.Dto.RequestDto;
using CataVentoApi.Entity.Enums;

namespace CataVentoApi.Repositories.Interface
{
    public interface ITaskRepository
    {
        Task<int> CreateTaskAsync(KanbanTask task);
        Task<KanbanTask?> GetTaskByIdAsync(int id);
        //Task<IEnumerable<KanbanTask>> GetAllTasksAsync();
        Task<IEnumerable<KanbanTask>> GetAllTasksAsync(KanbanBoardTypeEnum boardType);
        Task<bool> UpdateTaskAsync(KanbanTask task);
        Task<bool> UpdateColumnAsync(int id, KanbanTaskStatusEnum newStatus, int newPosition);
        Task<bool> ReorderTasksAsync(List<UpdatePriorityRequestDto> tasks);
        Task<bool> DeleteTaskAsync(int id);
    }
}
