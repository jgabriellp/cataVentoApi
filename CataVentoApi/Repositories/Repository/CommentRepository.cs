using CataVentoApi.DataContext;
using CataVentoApi.Entity;
using CataVentoApi.Repositories.Interface;
using Dapper;
using System.ComponentModel.Design;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CataVentoApi.Repositories.Repository
{
    public class CommentRepository : ICommentRepository
    {
        private readonly DapperContext _connection;

        public CommentRepository(DapperContext connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<Comment>> GetCommentsByPostIdsAsync(IEnumerable<long> postIds)
        {
            const string query = "SELECT \"CommentId\", \"PostId\", \"CreatorId\" FROM \"Comment\" WHERE \"PostId\" = ANY(@PostIds)";

            var postIdsList = postIds.ToList();

            using (var connection = _connection.CreateConnection())
            {
                var comments = await connection.QueryAsync<Comment>(
                    query,
                    new { PostIds = postIdsList }
                );

                return comments;
            }
        }

        public async Task<Comment?> GetCommentByIdAsync(long commentId)
        {
            const string query = "SELECT * FROM \"Comment\" WHERE \"CommentId\" = @CommentId";

            using (var connection = _connection.CreateConnection())
            {
                var comment = await connection.QueryFirstOrDefaultAsync<Comment>(query, new { CommentId = commentId });
                return comment;
            }
        }

        public async Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(long postId)
        {
            const string query = "SELECT * FROM \"Comment\" WHERE \"PostId\" = @PostId ORDER BY \"Date\" DESC";

            using (var connection = _connection.CreateConnection())
            {
                var comments = await connection.QueryAsync<Comment>(query, new { PostId = postId });
                return comments;
            }
        }

        public async Task<Comment> CreateCommentAsync(Comment comment)
        {
            const string query = @"
                INSERT INTO ""Comment"" (""Content"", ""Date"", ""PostId"", ""CreatorId"")
                VALUES (@Content, @Date, @PostId, @CreatorId)
                RETURNING ""CommentId"";";

            using (var connection = _connection.CreateConnection())
            {
                var commentId = await connection.ExecuteScalarAsync<long>(query, comment);
                comment.CommentId = commentId;
                return comment;
            }
        }

        public async Task<bool> UpdateCommentAsync(Comment comment)
        {
            const string query = @"
                UPDATE ""Comment""
                SET ""Content"" = @Content,
                    ""Date"" = @Date,
                    ""PostId"" = @PostId,
                    ""CreatorId"" = @CreatorId
                WHERE ""CommentId"" = @CommentId";


            using (var connection = _connection.CreateConnection())
            {
                var affectedRows = await connection.ExecuteAsync(query, comment);
                return affectedRows > 0;
            }
        }

        public async Task<bool> DeleteCommentAsync(long commentId)
        {
            const string query = "DELETE FROM \"Comment\" WHERE \"CommentId\" = @CommentId";

            using (var connection = _connection.CreateConnection())
            {
                var affectedRows = await connection.ExecuteAsync(query, new { CommentId = commentId });
                return affectedRows > 0;
            }
        }
    }
}
