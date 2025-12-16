using CataVentoApi.DataContext;
using CataVentoApi.Repositories.Interface;
using Dapper;
using System.Collections.Generic;
using System.Data;
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
            // AJUSTE: Usando aspas duplas ("") para nomes de tabela e colunas.
            const string sql = @"
                SELECT ""AudienceRole""
                FROM ""NoticeAudience""
                WHERE ""NoticeId"" = @NoticeId";

            using (var connection = _connection.CreateConnection())
            {
                return await connection.QueryAsync<short>(sql, new { NoticeId = noticeId });
            }
        }

        public async Task<bool> AddAudiencesAsync(
            long noticeId,
            IEnumerable<short> audienceRoles,
            IDbConnection connection,
            IDbTransaction transaction)
        {
            var parameters = audienceRoles
                .Select(role => new { NoticeId = noticeId, AudienceRole = role })
                .ToList();

            const string sql = @"
                INSERT INTO ""NoticeAudience"" (""NoticeId"", ""AudienceRole"")
                VALUES (@NoticeId, @AudienceRole);";

            // 🚨 MUDANÇA CRUCIAL: 
            // 1. Usa a 'connection' passada.
            // 2. Passa a 'transaction' para o Dapper.
            // 3. NÃO CRIA UMA NOVA CONEXÃO.
            var rowsAffected = await connection.ExecuteAsync(sql, parameters, transaction);

            return rowsAffected == parameters.Count;
        }

        public async Task<bool> AddAudiencesAsync(long noticeId, IEnumerable<short> audienceRoles)
        {
            // Mapeia a lista de Roles (short) para objetos anônimos com NoticeId
            var parameters = audienceRoles
                .Select(role => new { NoticeId = noticeId, AudienceRole = role })
                .ToList();

            // AJUSTE: Usando aspas duplas ("")
            const string sql = @"
                INSERT INTO ""NoticeAudience"" (""NoticeId"", ""AudienceRole"")
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
            // AJUSTE: Usando aspas duplas ("")
            const string sql = @"DELETE FROM ""NoticeAudience"" WHERE ""NoticeId"" = @NoticeId";

            using (var connection = _connection.CreateConnection())
            {
                // A exclusão de 0 linhas ainda é um sucesso lógico.
                await connection.ExecuteAsync(sql, new { NoticeId = noticeId });
                return true;
            }
        }

        public async Task<bool> HasRoleAsync(long noticeId, short audienceRole)
        {
            // AJUSTE: Usando aspas duplas ("")
            const string sql = @"
                SELECT 1 
                FROM ""NoticeAudience"" 
                WHERE ""NoticeId"" = @NoticeId AND ""AudienceRole"" = @AudienceRole";

            using (var connection = _connection.CreateConnection())
            {
                var result = await connection.QueryFirstOrDefaultAsync<int?>(sql, new { NoticeId = noticeId, AudienceRole = audienceRole });

                return result.HasValue;
            }
        }
    }
}