namespace CataVentoApi.Entity
{
    public class Comment
    {
        public long CommentId { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public long PostId { get; set; }
        public long CreatorId { get; set; }
    }
}
