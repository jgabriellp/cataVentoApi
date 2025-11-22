namespace CataVentoApi.Entity.Dto.RequestDto
{
    public class CommentRequestDto
    {
        public string Content { get; set; }
        public long PostId { get; set; }
        public long CreatorId { get; set; }
    }
}
