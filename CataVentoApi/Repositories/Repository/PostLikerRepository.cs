using CataVentoApi.DataContext;
using CataVentoApi.Repositories.Interface;
using Dapper;

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
                SELECT PostId, UsuarioId 
                FROM PostLiker 
                WHERE PostId IN @PostIds";

            using (var connection = _connection.CreateConnection())
            {
                // Retorna uma lista de tuplas para simplificar o mapeamento
                var likerAssociations = await connection.QueryAsync<
                    (long PostId, long UsuarioId)>(
                        query,
                        new { PostIds = postIds }
                    );

                return likerAssociations;
            }
        }

        public async Task AddLikeAsync(long postId, long usuarioId)
        {
            const string query = @"
                INSERT INTO PostLiker (PostId, UsuarioId)
                VALUES (@PostId, @UsuarioId);";

            using(var connection = _connection.CreateConnection())
            {
                await connection.ExecuteAsync(query, new { PostId = postId, UsuarioId = usuarioId });
            }
        }

        public async Task<bool> HasUserLikedAsync(long postId, long usuarioId)
        {
            const string query = "SELECT COUNT(1) FROM PostLiker WHERE PostId = @PostId AND UsuarioId = @UsuarioId";

            using (var connection = _connection.CreateConnection())
            {
                var count = await connection.ExecuteScalarAsync<int>(query, new { PostId = postId, UsuarioId = usuarioId });
                return count > 0;
            }
        }

        public async Task RemoveLikeAsync(long postId, long usuarioId)
        {
            const string query = @"
                DELETE FROM PostLiker
                WHERE PostId = @PostId AND UsuarioId = @UsuarioId;";

            using (var connection = _connection.CreateConnection())
            {
                await connection.ExecuteAsync(query, new { PostId = postId, UsuarioId = usuarioId });
            }
        }

        public async Task<bool> HasUserLikedIconAsync(long postId, long usuarioId)
        {
            const string query = @"
                SELECT COUNT(1) 
                FROM PostLiker 
                WHERE PostId = @PostId AND UsuarioId = @UsuarioId";

            using (var connection = _connection.CreateConnection())
            {
                var count = await connection.ExecuteScalarAsync<int>(
                    query,
                    new { PostId = postId, UsuarioId = usuarioId }
                );

                return count > 0;
            }
        }
    }
}
