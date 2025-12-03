namespace CataVentoApi.Entity.Dto.RequestDto
{
    public class NoticeResquestDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; }
        public string? PhotoUrl { get; set; }
        public long CreatorId { get; set; }
        public ICollection<NoticeAudienceRequestDto> Audiences { get; set; } = new List<NoticeAudienceRequestDto>();
    }
}
