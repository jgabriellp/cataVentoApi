using CataVentoApi.Entity;
using CataVentoApi.Entity.Dto.RequestDto;
using CataVentoApi.Repositories.Interface;
using CataVentoApi.Services.Interface;

namespace CataVentoApi.Services.Service
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IPostRepository _postRepository;
        private readonly IUsuarioRepository _usarioRepository;

        public CommentService(ICommentRepository commentRepository, IPostRepository postRepository, IUsuarioRepository usarioRepository)
        {
            _commentRepository = commentRepository;
            _postRepository = postRepository;
            _usarioRepository = usarioRepository;
        }

        public async Task<Comment?> GetCommentByIdAsync(long commentId)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(commentId);
            if (comment == null)
            {
                return null;
            }
            return comment;
        }

        public async Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(long postId)
        {
            var comments = await _commentRepository.GetCommentsByPostIdAsync(postId);
            return comments;
        }

        public async Task<Comment> CreateCommentAsync(CommentRequestDto commentRequestDto)
        {
            var postId = await _postRepository.GetPostByIdAsync(commentRequestDto.PostId);
            var userId = await _usarioRepository.GetById(commentRequestDto.CreatorId);
            
            if (postId == null || userId == null)
            {
                return null;
            }

            var newComment = new Comment
            {
                Content = commentRequestDto.Content,
                Date = DateTime.UtcNow,
                PostId = commentRequestDto.PostId,
                CreatorId = commentRequestDto.CreatorId,
            };

            return await _commentRepository.CreateCommentAsync(newComment);
        }

        public async Task<bool> UpdateCommentAsync(long commentId, CommentRequestDto commentRequestDto)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(commentId);
            var postId = await _postRepository.GetPostByIdAsync(commentRequestDto.PostId);
            var userId = await _usarioRepository.GetById(commentRequestDto.CreatorId);

            if (comment == null || postId == null || userId == null)
            {
                return false;
            }

            var updatedComment = new Comment
            {
                CommentId = commentId,
                Content = commentRequestDto.Content,
                Date = comment.Date,
                PostId = commentRequestDto.PostId,
                CreatorId = commentRequestDto.CreatorId,
            };

            return await _commentRepository.UpdateCommentAsync(updatedComment);
        }

        public async Task<bool> DeleteCommentAsync(long commentId)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(commentId);

            if (comment == null) return false;

            return await _commentRepository.DeleteCommentAsync(commentId);
        }
    }
}
