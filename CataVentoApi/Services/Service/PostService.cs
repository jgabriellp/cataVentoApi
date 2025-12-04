using CataVentoApi.Entity;
using CataVentoApi.Entity.Dto.RequestDto;
using CataVentoApi.Repositories.Interface;
using CataVentoApi.Repositories.Repository;
using CataVentoApi.Services.Interface;
using System.Net;

namespace CataVentoApi.Services.Service
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IUsuarioRepository _userRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IPostLikerRepository _postLikerRepository;

        public PostService(IPostRepository postRepository, IGroupRepository groupRepository, 
            IUsuarioRepository usuarioRepository, ICommentRepository commentRepository, IPostLikerRepository postLikerRepository)
        {
            _postRepository = postRepository;
            _groupRepository = groupRepository;
            _userRepository = usuarioRepository;
            _commentRepository = commentRepository;
            _postLikerRepository = postLikerRepository;
        }

        public async Task<Post?> GetPostByIdAsync(long postId)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            
            if (post == null)
            {
                return null;
            }
           
            return post;
        }

        public async Task<IEnumerable<Post>> GetPostsByUserIdAsync(long userId, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 5;

            var posts = await _postRepository.GetPostsByUserIdAsync(userId, pageNumber, pageSize);
            
            if (!posts.Any()) return Enumerable.Empty<Post>();

            var postIds = posts.Select(p => p.PostId);

            var comments = await _commentRepository.GetCommentsByPostIdsAsync(postIds);

            var likers = await _postLikerRepository.GetLikersByPostIdsAsync(postIds);

            var commentsLookup = comments
                .ToLookup(c => c.PostId, c => c.CommentId);

            var likersLookup = likers
                .ToLookup(l => l.PostId, l => l.UsuarioId);

            foreach (var post in posts)
            {
                // Liga os Comentários
                post.CommentsIds = commentsLookup[post.PostId].ToList();
                // Liga os Likes
                post.LikersIds = likersLookup[post.PostId].ToList(); 
            }
            return posts;
        }

        public async Task<IEnumerable<Post>> GetPostsByGroupIdAsync(long groupId, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 5;

            var posts = await _postRepository.GetPostsByGroupIdAsync(groupId, pageNumber, pageSize);
            
            if (!posts.Any()) return Enumerable.Empty<Post>();

            var postIds = posts.Select(p => p.PostId).ToList();

            var comments = await _commentRepository.GetCommentsByPostIdsAsync(postIds);

            var likers = await _postLikerRepository.GetLikersByPostIdsAsync(postIds);

            var commentsLookup = comments
                .ToLookup(c => c.PostId, c => c.CommentId);

            var likersLookup = likers
                .ToLookup(l => l.PostId, l => l.UsuarioId);

            foreach (var post in posts)
            {
                // Liga os Comentários
                post.CommentsIds = commentsLookup[post.PostId].ToList();

                // Liga os Likes
                post.LikersIds = likersLookup[post.PostId].ToList(); 
            }

            return posts;
        }

        public async Task<Post> CreatePostAsync(PostRequestDto postRequestDto)
        {
            var existingGroupId = await _groupRepository.GetGroupByIdAsync(postRequestDto.GroupId);
            var existingCreatorId = await _userRepository.GetById(postRequestDto.CreatorId);

            if (existingGroupId == null || existingCreatorId == null)
            {
                return null;
            }

            var newPost = new Post
            {
                Content = postRequestDto.Content,
                Date = DateTime.UtcNow,
                GroupId = postRequestDto.GroupId,
                CreatorId = postRequestDto.CreatorId,
                ImageUrl = postRequestDto.ImageUrl,
                LikersIds = new List<long>(),
                CommentsIds = new List<long>()
            };

            return await _postRepository.CreatePostAsync(newPost);
        }

        public async Task<bool> UpdatePostAsync(long postId, PostRequestDto postRequestDto)
        {
            var updatePost = await _postRepository.GetPostByIdAsync(postId);

            if (updatePost == null)
            {
                return false;
            }

            var existingGroupId = await _groupRepository.GetGroupByIdAsync(postRequestDto.GroupId);
            var existingCreatorId = await _userRepository.GetById(postRequestDto.CreatorId);

            if (existingGroupId == null || existingCreatorId == null)
            {
                return false;
            }

            updatePost.Content = postRequestDto.Content;
            updatePost.ImageUrl = postRequestDto.ImageUrl;
            updatePost.GroupId = postRequestDto.GroupId;

            return await _postRepository.UpdatePostAsync(updatePost);
        }

        public async Task<bool> DeletePostAsync(long postId)
        {
            var existingPost = await _postRepository.GetPostByIdAsync(postId);

            if (existingPost == null)
            {
                return false;
            }

            return await _postRepository.DeletePostAsync(postId);
        }
    }
}
