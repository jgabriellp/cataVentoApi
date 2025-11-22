using CataVentoApi.Entity;

namespace CataVentoApi.Repositories.Interface
{
    public interface ICommentRepository
    {
        Task<IEnumerable<Comment>> GetCommentsByPostIdsAsync(IEnumerable<long> postIds);
        Task<Comment?> GetCommentByIdAsync(long commentId);
        Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(long postId);
        Task<Comment> CreateCommentAsync(Comment comment);
        Task<bool> UpdateCommentAsync(Comment comment);
        Task<bool> DeleteCommentAsync(long commentId);
    }
}
