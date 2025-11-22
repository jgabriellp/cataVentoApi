namespace CataVentoApi.Repositories.Interface
{
    public interface IPostLikerRepository
    {
        Task AddLikeAsync(long postId, long usuarioId);
        Task RemoveLikeAsync(long postId, long usuarioId);
        Task<bool> HasUserLikedAsync(long postId, long usuarioId);
        Task<IEnumerable<(long PostId, long UsuarioId)>> GetLikersByPostIdsAsync(IEnumerable<long> postIds);
        Task<bool> HasUserLikedIconAsync(long postId, long usuarioId);
    }
}
