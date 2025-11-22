using CataVentoApi.DataContext;
using CataVentoApi.Entity;
using CataVentoApi.Repositories.Interface;
using Dapper;

namespace CataVentoApi.Repositories.Repository
{
    public class PostRepository : IPostRepository
    {
        private readonly DapperContext _connection;

        public PostRepository(DapperContext connection)
        {
            _connection = connection;
        }

        public async Task<Post?> GetPostByIdAsync(long postId)
        {
            const string query = "SELECT * FROM Post WHERE PostId = @PostId";
            const string sqlAssociations = @"
                SELECT P.*, 
                       L.UsuarioId AS LikersIds,
                       C.CommentId AS CommentsIds
                FROM Post P
                LEFT JOIN PostLiker L ON P.PostId = L.PostId
                LEFT JOIN [Comment] C ON P.PostId = C.PostId
                WHERE P.PostId = @PostId";

            using (var connection = _connection.CreateConnection())
            {
                var post = await connection.QueryFirstOrDefaultAsync<Post>(query, new { PostId = postId });
                var associations = await connection.QueryAsync<dynamic>(sqlAssociations, new { PostId = postId });

                if (post != null)
                {
                    post.LikersIds = associations
                        .Where(a => a.LikersIds != null)
                        .Select(a => (long)a.LikersIds)
                        .Distinct()
                        .ToList();
                    post.CommentsIds = associations
                        .Where(a => a.CommentsIds != null)
                        .Select(a => (long)a.CommentsIds)
                        .Distinct()
                        .ToList();
                }

                return post;
            }
        }

        public async Task<IEnumerable<Post>> GetPostsByGroupIdAsync(long groupId, int pageNumber, int pageSize)
        {
            //const string query = "SELECT * FROM Post WHERE GroupId = @GroupId ORDER BY [Date] DESC";
            int offset = (pageNumber - 1) * pageSize;
            
            const string query = @"
                SELECT * FROM Post 
                WHERE GroupId = @GroupId 
                ORDER BY [Date] DESC 
                OFFSET @Offset ROWS 
                FETCH NEXT @PageSize ROWS ONLY;
    ";

            using (var connection = _connection.CreateConnection())
            {
                //var posts = await connection.QueryAsync<Post>(query, new { GroupId = groupId });
                var posts = await connection.QueryAsync<Post>(
                    query,
                    new
                    {
                        GroupId = groupId,
                        Offset = offset,     // O ponto de partida da página
                        PageSize = pageSize  // O número de itens na página
                    }
                );

                return posts;
            }
        }

        public async Task<Post> CreatePostAsync(Post post)
        {
            const string query = @"
                INSERT INTO Post (Content, Date, GroupId, CreatorId, ImageUrl)
                VALUES (@Content, @Date, @GroupId, @CreatorId, @ImageUrl);
                SELECT CAST(SCOPE_IDENTITY() as bigint);";

            using (var connection = _connection.CreateConnection())
            {
                var postId = await connection.ExecuteScalarAsync<long>(query, post);
                post.PostId = postId;
                return post;
            }
        }

        public async Task<bool> UpdatePostAsync(Post post)
        {
            const string query = @"
                UPDATE Post
                SET Content = @Content,
                    Date = @Date,
                    GroupId = @GroupId,
                    CreatorId = @CreatorId,
                    ImageUrl = @ImageUrl
                WHERE PostId = @PostId";

            using (var connection = _connection.CreateConnection())
            {
                var rowsAffected = await connection.ExecuteAsync(query, post);
                return rowsAffected > 0;
            }
        }

        public async Task<bool> DeletePostAsync(long postId)
        {
            const string query = "DELETE FROM Post WHERE PostId = @PostId";
            using (var connection = _connection.CreateConnection())
            {
                var rowsAffected = await connection.ExecuteAsync(query, new { PostId = postId });
                return rowsAffected > 0;
            }
        }
    }
}
