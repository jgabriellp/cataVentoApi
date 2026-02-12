using CataVentoApi.Entity;
using CataVentoApi.Entity.Dto.RequestDto;
using CataVentoApi.Entity.Enums;

namespace CataVentoApi.Services.Interface
{
    public interface ITaskService
    {
        Task<int> CreateTask(KanbanTask task);
        Task<KanbanTask?> GetTaskById(int id);
        Task<IEnumerable<KanbanTask>> GetAllTasks(KanbanBoardTypeEnum boardType);
        Task<KanbanTask?> UpdateTask(KanbanTask task);
        Task<bool> UpdateColumn(int id, KanbanTaskStatusEnum newStatus, int newPosition);
        Task<bool> ReorderTasks(List<UpdatePriorityRequestDto> tasks);
        Task<bool> DeleteTask(int id);
    }
}