using CataVentoApi.Entity;
using CataVentoApi.Entity.Dto.RequestDto;

namespace CataVentoApi.Services.Interface
{
    public interface ICommentService
    {
        Task<Comment?> GetCommentByIdAsync(long commentId);
        Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(long postId);
        Task<Comment> CreateCommentAsync(CommentRequestDto commentRequestDto);
        Task<bool> UpdateCommentAsync(long commentId, CommentRequestDto commentRequestDto);
        Task<bool> DeleteCommentAsync(long commentId);
    }
}
