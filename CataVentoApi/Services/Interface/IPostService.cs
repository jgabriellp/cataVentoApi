using CataVentoApi.Entity;
using CataVentoApi.Entity.Dto.RequestDto;

namespace CataVentoApi.Services.Interface
{
    public interface IPostService
    {
        Task<Post?> GetPostByIdAsync(long postId);
        Task<Post?> GetPostByContentAsync(string content);
        Task<IEnumerable<Post>> GetPostsByGroupIdAsync(long groupId, int pageNumber, int pageSize);
        Task<IEnumerable<Post>> GetPostsByGroupIdAndDateAsync(long groupId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<Post>> GetPostsByUserIdAsync(long userId, int pageNumber, int pageSize);
        Task<Post> CreatePostAsync(PostRequestDto postRequestDto);
        Task<bool> UpdatePostAsync(long postId, PostRequestDto postRequestDto);
        Task<bool> PatchPostImageUrlAsync(long postId, string imageUrl);
        Task<bool> DeletePostAsync(long postId);
    }
}
