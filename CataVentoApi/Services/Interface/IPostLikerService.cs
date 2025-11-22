using CataVentoApi.Entity.Dto.RequestDto;

namespace CataVentoApi.Services.Interface
{
    public interface IPostLikerService
    {
        Task<bool> ToggleLikeAsync(PostLikerRequestDto request);
        Task<bool> HasUserLikedIconAsync(long postId, long usuarioId);
    }
}
