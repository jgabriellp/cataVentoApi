using CataVentoApi.DataContext;
using CataVentoApi.Entity;
using CataVentoApi.Repositories.Interface;
using Dapper;

namespace CataVentoApi.Repositories.Repository
{
    public class NoticeRepository : INoticeRepository
    {

        private readonly DapperContext _connection;
        private readonly INoticeAudienceRepository _audienceRepository;

        public NoticeRepository(DapperContext connection, INoticeAudienceRepository audienceRepository)
        {
            _connection = connection;
            _audienceRepository = audienceRepository;
        }

        private async Task LoadAudiencesForNotices(IEnumerable<Notice> notices)
        {
            // Otimização: Se você tivesse muitos avisos, seria melhor carregar todos os
            // NoticeAudience de uma vez e mapeá-los em memória (Multi-query).
            // Por simplicidade, vamos iterar:

            var loadTasks = notices.Select(async notice =>
            {
                var audienceRoles = await _audienceRepository.GetRolesByNoticeIdAsync(notice.NoticeId);
                notice.Audiences = audienceRoles
                    .Select(roleValue => new NoticeAudience
                    {
                        NoticeId = notice.NoticeId,
                        AudienceRole = roleValue
                    })
                    .ToList();
            });

            await Task.WhenAll(loadTasks);
        }

        public async Task<Notice> GetByIdAsync(long noticeId)
        {
            const string query = @"SELECT * FROM ""Notice"" WHERE ""NoticeId"" = @NoticeId"; ;

            using (var connection = _connection.CreateConnection())
            {
                var notice = await connection.QueryFirstOrDefaultAsync<Notice>(query, new { NoticeId = noticeId });

                if (notice != null)
                {
                    var audienceRoles = await _audienceRepository.GetRolesByNoticeIdAsync(noticeId);

                    notice.Audiences = audienceRoles
                        .Select(roleValue => new NoticeAudience
                        {
                            NoticeId = noticeId,
                            AudienceRole = roleValue
                        })
                        .ToList();
                }

                return notice;
            }
        }

        public async Task<IEnumerable<Notice>> GetAllAsync(int pageNumber, int pageSize)
        {
            int offset = (pageNumber - 1) * pageSize;

            const string query = @"
                SELECT * FROM ""Notice""
                ORDER BY ""DateCreated"" DESC
                LIMIT @PageSize OFFSET @Offset;";

            using (var connection = _connection.CreateConnection())
            {
                var notices = await connection.QueryAsync<Notice>(
                    query,
                    new { Offset = offset, PageSize = pageSize }
                );

                // Carrega as associações
                await LoadAudiencesForNotices(notices);

                return notices;
            }
        }

        public async Task<IEnumerable<Notice>> GetAllActiveAsync(int pageNumber, int pageSize)
        {
            int offset = (pageNumber - 1) * pageSize;

            const string query = @"
                SELECT * FROM ""Notice""
                WHERE ""IsActive"" = TRUE
                ORDER BY ""DateCreated"" DESC
                LIMIT @PageSize OFFSET @Offset;";

            using (var connection = _connection.CreateConnection())
            {
                var notices = await connection.QueryAsync<Notice>(
                    query,
                    new { Offset = offset, PageSize = pageSize }
                );

                // Carrega as associações
                await LoadAudiencesForNotices(notices);

                return notices;
            }
        }

        public async Task<IEnumerable<Notice>> GetByCreatorIdAsync(long creatorId, int pageNumber, int pageSize)
        {
            int offset = (pageNumber - 1) * pageSize;

            const string query = @"
                SELECT * FROM ""Notice""
                WHERE ""CreatorId"" = @CreatorId
                ORDER BY ""DateCreated"" DESC
                LIMIT @PageSize OFFSET @Offset;";

            using (var connection = _connection.CreateConnection())
            {
                var notices = await connection.QueryAsync<Notice>(
                    query,
                    new { CreatorId = creatorId, Offset = offset, PageSize = pageSize }
                );

                // Carrega as associações
                await LoadAudiencesForNotices(notices);

                return notices;
            }
        }

        public async Task<IEnumerable<Notice>> GetFilteredAsync(string? title, bool? isActive)
        {
            var sql = @"SELECT * FROM ""Notice"" WHERE 1=1";
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(title))
            {
                sql += @" AND ""Title"" ILIKE @Title";
                parameters.Add("Title", $"%{title}%");
            }

            if (isActive.HasValue)
            {
                sql += @" AND ""IsActive"" = @IsActive";
                parameters.Add("IsActive", isActive.Value);
            }

            sql += " ORDER BY DateCreated DESC";

            // NOTE: Este método está sem paginação conforme sua interface.
            // Se houver muitos dados, o desempenho pode ser afetado.

            using (var connection = _connection.CreateConnection())
            {
                var notices = await connection.QueryAsync<Notice>(sql, parameters);

                // Carrega as associações
                await LoadAudiencesForNotices(notices);

                return notices;
            }
        }

        public async Task<long> AddAsync(Notice notice)
        {
            const string query = @"
                INSERT INTO ""Notice"" (""Title"", ""Content"", ""IsActive"", ""DateCreated"", ""PhotoUrl"", ""CreatorId"")
                VALUES (@Title, @Content, @IsActive, @DateCreated, @PhotoUrl, @CreatorId)
                RETURNING ""NoticeId"";";

            using (var connection = _connection.CreateConnection())
            {
                // Inicia uma transação para garantir que Aviso e Público sejam inseridos
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Insere o Aviso principal e retorna o ID
                        var noticeId = await connection.ExecuteScalarAsync<long>(query, notice, transaction);

                        // 2. Adiciona as associações de Público (Roles)
                        var rolesToAdd = notice.Audiences.Select(a => a.AudienceRole).Distinct();
                        if (rolesToAdd.Any())
                        {
                            //await _audienceRepository.AddAudiencesAsync(noticeId, rolesToAdd);
                            await _audienceRepository.AddAudiencesAsync(
                                noticeId,
                                rolesToAdd,
                                connection,
                                transaction
                            );
                        }

                        transaction.Commit();
                        return noticeId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<bool> UpdateAsync(Notice notice)
        {
            const string query = @"
                UPDATE ""Notice""
                SET ""Title"" = @Title,
                    ""Content"" = @Content,
                    ""IsActive"" = @IsActive,
                    ""PhotoUrl"" = @PhotoUrl
                WHERE ""NoticeId"" = @NoticeId;";

            using (var connection = _connection.CreateConnection())
            {
                // Inicia transação para atualizar o aviso e suas associações
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Atualiza o Aviso principal
                        var rowsAffected = await connection.ExecuteAsync(query, notice, transaction);

                        // 2. Remove todas as associações antigas
                        await _audienceRepository.RemoveAllAudiencesAsync(notice.NoticeId);

                        // 3. Adiciona as novas associações
                        var rolesToAdd = notice.Audiences.Select(a => a.AudienceRole).Distinct();
                        if (rolesToAdd.Any())
                        {
                            await _audienceRepository.AddAudiencesAsync(notice.NoticeId, rolesToAdd, connection, transaction);
                        }

                        transaction.Commit();
                        return rowsAffected > 0;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<bool> DeleteAsync(long noticeId)
        {
            // O DELETE CASCADE no SQL deve cuidar da tabela NoticeAudience, mas
            // deletar o Notice principal é o suficiente.
            const string query = @"DELETE FROM ""Notice"" WHERE ""NoticeId"" = @NoticeId";

            using (var connection = _connection.CreateConnection())
            {
                var rowsAffected = await connection.ExecuteAsync(query, new { NoticeId = noticeId });
                return rowsAffected > 0;
            }
        }


    }
}
