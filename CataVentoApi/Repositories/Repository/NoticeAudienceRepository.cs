using CataVentoApi.DataContext;
using CataVentoApi.Repositories.Interface;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CataVentoApi.Repositories.Repository
{
    public class NoticeAudienceRepository : INoticeAudienceRepository
    {
        private readonly DapperContext _connection;

        public NoticeAudienceRepository(DapperContext connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<short>> GetRolesByNoticeIdAsync(long noticeId)
        {
            const string sql = @"
                SELECT AudienceRole
                FROM NoticeAudience
                WHERE NoticeId = @NoticeId";

            using (var connection = _connection.CreateConnection())
            {
                return await connection.QueryAsync<short>(sql, new { NoticeId = noticeId });
            }
        }

        public async Task<bool> AddAudiencesAsync(long noticeId, IEnumerable<short> audienceRoles)
        {
            // Mapeia a lista de Roles (short) para objetos anônimos com NoticeId
            // Isso permite que o Dapper execute a inserção em lote.
            var parameters = audienceRoles
                .Select(role => new { NoticeId = noticeId, AudienceRole = role })
                .ToList();

            const string sql = @"
                INSERT INTO NoticeAudience (NoticeId, AudienceRole)
                VALUES (@NoticeId, @AudienceRole);";

            using (var connection = _connection.CreateConnection())
            {
                // Dapper executa a query N vezes, uma para cada objeto na lista 'parameters'.
                var rowsAffected = await connection.ExecuteAsync(sql, parameters);

                // Verifica se todas as linhas esperadas foram inseridas
                return rowsAffected == parameters.Count;
            }
        }

        public async Task<bool> RemoveAllAudiencesAsync(long noticeId)
        {
            // Deleta todas as linhas na tabela de junção para o NoticeId especificado
            const string sql = "DELETE FROM NoticeAudience WHERE NoticeId = @NoticeId";

            using (var connection = _connection.CreateConnection())
            {
                // Se for executado com sucesso, retornamos true. 
                // A exclusão de 0 linhas ainda é um sucesso lógico.
                await connection.ExecuteAsync(sql, new { NoticeId = noticeId });
                return true;
            }
        }

        public async Task<bool> HasRoleAsync(long noticeId, short audienceRole)
        {
            // Verifica se a associação específica existe.
            const string sql = @"
                SELECT 1 
                FROM NoticeAudience 
                WHERE NoticeId = @NoticeId AND AudienceRole = @AudienceRole";

            using (var connection = _connection.CreateConnection())
            {
                // QueryFirstOrDefaultAsync<int?> retorna null se não houver registros.
                var result = await connection.QueryFirstOrDefaultAsync<int?>(sql, new { NoticeId = noticeId, AudienceRole = audienceRole });

                // Se o resultado não for nulo, a associação existe.
                return result.HasValue;
            }
        }
    }
}