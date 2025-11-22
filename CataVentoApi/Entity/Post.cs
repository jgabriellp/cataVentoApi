namespace CataVentoApi.Entity
{
    public class Post
    {
        public long PostId { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public long GroupId { get; set; }
        public long CreatorId { get; set; }
        public string ImageUrl { get; set; }
        public List<long> LikersIds { get; set; } = new List<long>();
        public List<long> CommentsIds { get; set; } = new List<long>();
    }
}
