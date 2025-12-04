using CataVentoApi.DataContext;
using CataVentoApi.Repositories.Interface;
using Dapper;
using System;

namespace CataVentoApi.Repositories.Repository
{
    public class PostLikerRepository : IPostLikerRepository
    {
        private readonly DapperContext _connection;

        public PostLikerRepository(DapperContext connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<(long PostId, long UsuarioId)>> GetLikersByPostIdsAsync(IEnumerable<long> postIds)
        {
            const string query = @"
                SELECT ""PostId"", ""UsuarioId"" 
                FROM ""PostLiker"" 
                WHERE ""PostId"" = ANY(@PostIds)";

            var postIdsList = postIds.ToList();

            using (var connection = _connection.CreateConnection())
            {
                var likerAssociations = await connection.QueryAsync<
                     (long PostId, long UsuarioId)>(
                         query,
                         new { PostIds = postIdsList }
                     );

                return likerAssociations;
            }
        }

        public async Task AddLikeAsync(long postId, long usuarioId)
        {
            const string query = @"
                INSERT INTO ""PostLiker"" (""PostId"", ""UsuarioId"") -- Aspas duplas na tabela e colunas
                VALUES (@PostId, @UsuarioId);";

            using (var connection = _connection.CreateConnection())
            {
                await connection.ExecuteAsync(query, new { PostId = postId, UsuarioId = usuarioId });
            }
        }

        public async Task<bool> HasUserLikedAsync(long postId, long usuarioId)
        {
            const string query = "SELECT COUNT(1) FROM \"PostLiker\" WHERE \"PostId\" = @PostId AND \"UsuarioId\" = @UsuarioId"; // Aspas duplas

            using (var connection = _connection.CreateConnection())
            {
                var count = await connection.ExecuteScalarAsync<int>(query, new { PostId = postId, UsuarioId = usuarioId });
                return count > 0;
            }
        }

        public async Task RemoveLikeAsync(long postId, long usuarioId)
        {
            const string query = @"
                DELETE FROM ""PostLiker"" -- Aspas duplas na tabela
                WHERE ""PostId"" = @PostId AND ""UsuarioId"" = @UsuarioId;";

            using (var connection = _connection.CreateConnection())
            {
                await connection.ExecuteAsync(query, new { PostId = postId, UsuarioId = usuarioId });
            }
        }

        public async Task<bool> HasUserLikedIconAsync(long postId, long usuarioId)
        {
            const string query = @"
                SELECT COUNT(1) 
                FROM ""PostLiker""
                WHERE ""PostId"" = @PostId AND ""UsuarioId"" = @UsuarioId";

            using (var connection = _connection.CreateConnection())
            {
                // ExecuteScalarAsync<int> funciona perfeitamente para COUNT(1)
                var count = await connection.ExecuteScalarAsync<int>(
                    query,
                    new { PostId = postId, UsuarioId = usuarioId }
                );

                return count > 0;
            }
        }
    }
}
