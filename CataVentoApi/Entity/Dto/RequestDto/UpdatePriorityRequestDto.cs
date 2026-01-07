using CataVentoApi.Entity.Enums;

namespace CataVentoApi.Entity.Dto.RequestDto
{
    public class UpdatePriorityRequestDto
    {
        public int Id { get; set; }
        public int Position { get; set; }
        public KanbanTaskStatusEnum Status { get; set; } // "todo", "inProgress", ou "done"
    }
}
