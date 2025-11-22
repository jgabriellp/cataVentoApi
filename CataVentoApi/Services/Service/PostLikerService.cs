using Azure.Core;
using CataVentoApi.Entity.Dto.RequestDto;
using CataVentoApi.Repositories.Interface;
using CataVentoApi.Services.Interface;

namespace CataVentoApi.Services.Service
{
    public class PostLikerService : IPostLikerService
    {
        private readonly IPostLikerRepository _postLikerRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPostRepository _postRepository;

        public PostLikerService(IPostLikerRepository postLikerRepository, 
            IUsuarioRepository usuarioRepository, IPostRepository postRepository)
        {
            _postLikerRepository = postLikerRepository;
            _usuarioRepository = usuarioRepository;
            _postRepository = postRepository;
        }

        public async Task<bool> HasUserLikedIconAsync(long postId, long usuarioId)
        {
            var user = await _usuarioRepository.GetById(usuarioId);
            var post = await _postRepository.GetPostByIdAsync(postId);

            if (user == null || post == null) return false;

            return await _postLikerRepository.HasUserLikedAsync(postId, usuarioId);
        }

        public async Task<bool> ToggleLikeAsync(PostLikerRequestDto request)
        {
            var user = await _usuarioRepository.GetById(request.UsuarioId);
            var post = await _postRepository.GetPostByIdAsync(request.PostId);

            if (user == null || post == null) return false;

            var hasLiked = await _postLikerRepository.HasUserLikedAsync(request.PostId, request.UsuarioId);

            if (hasLiked)
            {
                // Se já curtiu, remove o like (descurtir)
                await _postLikerRepository.RemoveLikeAsync(request.PostId, request.UsuarioId);
                return true;
            }
            else
            {
                // Se não curtiu, adiciona o like (curtir)
                await _postLikerRepository.AddLikeAsync(request.PostId, request.UsuarioId);
                return true;
            }
        }
    }
}
