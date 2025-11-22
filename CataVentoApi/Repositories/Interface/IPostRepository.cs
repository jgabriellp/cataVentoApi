using CataVentoApi.Entity;

namespace CataVentoApi.Repositories.Interface
{
    public interface IPostRepository
    {
        Task<Post?> GetPostByIdAsync(long postId);
        Task<IEnumerable<Post>> GetPostsByGroupIdAsync(long groupId, int pageNumber, int pageSize);
        Task<Post> CreatePostAsync(Post post);
        Task<bool> UpdatePostAsync(Post post);
        Task<bool> DeletePostAsync(long postId);
    }
}
