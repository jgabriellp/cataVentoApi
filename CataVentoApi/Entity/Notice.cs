namespace CataVentoApi.Entity
{
    public class Notice
    {
        public long NoticeId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; }
        public string? PhotoUrl { get; set; }
        public long CreatorId { get; set; }
        public ICollection<NoticeAudience> Audiences { get; set; } = new List<NoticeAudience>();
    }
}
