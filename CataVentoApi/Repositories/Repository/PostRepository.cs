using CataVentoApi.DataContext;
using CataVentoApi.Entity;
using CataVentoApi.Repositories.Interface;
using Dapper;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            const string query = "SELECT * FROM \"Post\" WHERE \"PostId\" = @PostId";

            const string sqlAssociations = @"
                SELECT 
                    P.*,
                    L.""UsuarioId"" AS LikersIds,
                    C.""CommentId"" AS CommentsIds
                FROM ""Post"" P
                LEFT JOIN ""PostLiker"" L ON P.""PostId"" = L.""PostId""
                LEFT JOIN ""Comment"" C ON P.""PostId"" = C.""PostId""
                WHERE P.""PostId"" = @PostId";

            using (var connection = _connection.CreateConnection())
            {
                var post = await connection.QueryFirstOrDefaultAsync<Post>(query, new { PostId = postId });

                var associations = await connection.QueryAsync<dynamic>(sqlAssociations, new { PostId = postId });

                if (post != null)
                {
                    post.LikersIds = associations
                        .Where(a => a.likersids != null)
                        .Select(a => (long)a.likersids)
                        .Distinct()
                        .ToList();

                    post.CommentsIds = associations
                        .Where(a => a.commentsids != null)
                        .Select(a => (long)a.commentsids)
                        .Distinct()
                        .ToList();
                }

                return post;
            }
        }

        public async Task<Post?> GetPostByContentAsync(string content)
        {
            const string query = "SELECT * FROM \"Post\" WHERE \"Content\" ILIKE @Content";

            const string sqlAssociations = @"
                SELECT 
                    P.*,
                    L.""UsuarioId"" AS LikersIds,
                    C.""CommentId"" AS CommentsIds
                FROM ""Post"" P
                LEFT JOIN ""PostLiker"" L ON P.""PostId"" = L.""PostId""
                LEFT JOIN ""Comment"" C ON P.""PostId"" = C.""PostId""
                WHERE P.""Content"" ILIKE @Content";

            using (var connection = _connection.CreateConnection())
            {
                var post = await connection.QueryFirstOrDefaultAsync<Post>(query, new { Content = $"%{content}%" });

                var associations = await connection.QueryAsync<dynamic>(sqlAssociations, new { Content = $"%{content}%" });

                if (post != null)
                {
                    post.LikersIds = associations
                        .Where(a => a.likersids != null)
                        .Select(a => (long)a.likersids)
                        .Distinct()
                        .ToList();

                    post.CommentsIds = associations
                        .Where(a => a.commentsids != null)
                        .Select(a => (long)a.commentsids)
                        .Distinct()
                        .ToList();
                }

                return post;
            }
        }

        public async Task<IEnumerable<Post>> GetPostsByGroupIdAndDateAsync(long groupId, DateTime startDate, DateTime endDate)
        {
            const string query = @"
                SELECT * FROM ""Post"" 
                WHERE ""GroupId"" = @GroupId 
                AND ""Date"" BETWEEN @StartDate AND @EndDate
                ORDER BY ""Date"" DESC;";

            using (var connection = _connection.CreateConnection())
            {
                var posts = await connection.QueryAsync<Post>(
                    query,
                    new
                    {
                        GroupId = groupId,
                        StartDate = startDate,
                        EndDate = endDate
                    }
                );

                return posts;
            }
        }

        public async Task<IEnumerable<Post>> GetPostsByGroupIdAsync(long groupId, int pageNumber, int pageSize)
        {
            int offset = (pageNumber - 1) * pageSize;

            const string query = @"
                SELECT * FROM ""Post"" 
                WHERE ""GroupId"" = @GroupId 
                ORDER BY ""Date"" DESC 
                LIMIT @PageSize OFFSET @Offset;";

            using (var connection = _connection.CreateConnection())
            {
                var posts = await connection.QueryAsync<Post>(
                    query,
                    new
                    {
                        GroupId = groupId,
                        Offset = offset,
                        PageSize = pageSize
                    }
                );

                return posts;
            }
        }

        public async Task<IEnumerable<Post>> GetPostsByUserIdAsync(long userId, int pageNumber, int pageSize)
        {
            int offset = (pageNumber - 1) * pageSize;

            const string query = @"
                SELECT * FROM ""Post"" 
                WHERE ""CreatorId"" = @CreatorId 
                ORDER BY ""Date"" DESC 
                LIMIT @PageSize OFFSET @Offset;";

            using (var connection = _connection.CreateConnection())
            {
                var posts = await connection.QueryAsync<Post>(
                    query,
                    new
                    {
                        CreatorId = userId,
                        Offset = offset,
                        PageSize = pageSize
                    }
                );

                return posts;
            }
        }

        public async Task<Post> CreatePostAsync(Post post)
        {
            const string query = @"
                INSERT INTO ""Post"" (""Content"", ""Date"", ""GroupId"", ""CreatorId"", ""ImageUrl"")
                VALUES (@Content, @Date, @GroupId, @CreatorId, @ImageUrl)
                RETURNING ""PostId"";";

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
                UPDATE ""Post"" -- Aspas duplas na tabela
                SET ""Content"" = @Content,
                    ""Date"" = @Date,
                    ""GroupId"" = @GroupId,
                    ""CreatorId"" = @CreatorId,
                    ""ImageUrl"" = @ImageUrl
                WHERE ""PostId"" = @PostId";


            using (var connection = _connection.CreateConnection())
            {
                var rowsAffected = await connection.ExecuteAsync(query, post);
                return rowsAffected > 0;
            }
        }

        public async Task<bool> DeletePostAsync(long postId)
        {
            const string query = "DELETE FROM \"Post\" WHERE \"PostId\" = @PostId";

            using (var connection = _connection.CreateConnection())
            {
                var rowsAffected = await connection.ExecuteAsync(query, new { PostId = postId });
                return rowsAffected > 0;
            }
        }
    }
}
