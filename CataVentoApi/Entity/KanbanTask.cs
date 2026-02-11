using CataVentoApi.Entity.Enums;

namespace CataVentoApi.Entity
{
    public class KanbanTask
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public long? UsuarioId { get; set; } // FK para a tabela Usuario
        public DateTime? DueDate { get; set; }
        public TaskPriorityEnum Priority { get; set; }
        public KanbanTaskStatusEnum Status { get; set; }
        public int Position { get; set; }
        public UsuarioResponsavel? Responsavel { get; set; }
        public KanbanBoardTypeEnum BoardType { get; set; }
    }
}
