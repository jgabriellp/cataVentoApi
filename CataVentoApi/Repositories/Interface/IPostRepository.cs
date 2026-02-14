using CataVentoApi.Entity;
using CataVentoApi.Entity.Dto.ResponseDto;

namespace CataVentoApi.Repositories.Interface
{
    public interface IPostRepository
    {
        Task<Post?> GetPostByIdAsync(long postId);
        Task<Post?> GetPostByContentAsync(string content);
        Task<IEnumerable<UserPostsCountResponseDto>> GetPostsCountByUserAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Post>> GetPostsByGroupIdAsync(long groupId, int pageNumber, int pageSize);
        Task<IEnumerable<Post>> GetPostsByGroupIdAndDateAsync(long groupId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<Post>> GetPostsByUserIdAsync(long userId, int pageNumber, int pageSize);
        Task<Post> CreatePostAsync(Post post);
        Task<bool> UpdatePostAsync(Post post);
        Task<bool> DeletePostAsync(long postId);
    }
}
