using CataVentoApi.DataContext;
using CataVentoApi.Entity;
using CataVentoApi.Entity.Dto.RequestDto;
using CataVentoApi.Entity.Dto.ResponseDto;
using CataVentoApi.Repositories.Interface;
using Dapper;
using System;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;

namespace CataVentoApi.Repositories.Repository
{
    public class GroupRepository : IGroupRepository
    {
        private readonly DapperContext _connection;

        public GroupRepository(DapperContext connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<Group>> GetAllGroupsAsync(int pageNumber, int pageSize)
        {
            const string orderByField = "GroupId";

            int offset = (pageNumber - 1) * pageSize;

            using (var connection = _connection.CreateConnection())
            {
                var parameters = new { Offset = offset, PageSize = pageSize };

                var sqlGroupsPaged = $@"
                    SELECT ""GroupId"", ""GroupName"" FROM ""Group""  -- Aspas duplas na tabela e colunas
                    ORDER BY ""{orderByField}"" ASC 
                    LIMIT @PageSize OFFSET @Offset; -- MUDANÇA CRÍTICA: Paginação PostgreSQL
                ";

                var groups = (await connection.QueryAsync<Group>(sqlGroupsPaged, parameters)).ToList();

                if (!groups.Any()) return Enumerable.Empty<Group>();

                var groupIds = groups.Select(g => g.GroupId).ToList();

                const string sqlAssociationsFiltered = @"
                    SELECT ""UsuarioId"", ""GroupId"" FROM ""UsuarioGroup"" -- Aspas duplas na tabela e colunas
                    WHERE ""GroupId"" = ANY(@GroupIds); -- Uso de ANY() para listas de parâmetros
                ";

                var associationParameters = new { GroupIds = groupIds };

                var associations = await connection.QueryAsync<
                     (long UsuarioId, int GroupId)>(sqlAssociationsFiltered, associationParameters);

                var groupedAssociations = associations
                    .ToLookup(a => a.GroupId, a => a.UsuarioId);

                foreach (var group in groups)
                {
                    group.UsuariosIds = groupedAssociations[group.GroupId].ToList();
                }

                return groups;
            }
        }

        public async Task<IEnumerable<Group>> GetAllGroupsAsync()
        {
            const string sqlGroups = "SELECT \"GroupId\", \"GroupName\" FROM \"Group\"";

            const string sqlAssociations = "SELECT \"UsuarioId\", \"GroupId\" FROM \"UsuarioGroup\"";

            using (var connection = _connection.CreateConnection())
            {
                var groups = (await connection.QueryAsync<Group>(sqlGroups)).ToList();

                var associations = await connection.QueryAsync<
                     (long UsuarioId, int GroupId)>(sqlAssociations);

                var groupedAssociations = associations
                    .GroupBy(a => a.GroupId)
                    .ToDictionary(g => g.Key, g => g.Select(a => a.UsuarioId).ToList());

                foreach (var group in groups)
                {
                    if (groupedAssociations.TryGetValue(group.GroupId, out var usuarioIds))
                    {
                        group.UsuariosIds = usuarioIds;
                    }
                }

                return groups;
            }
        }

        public async Task<Group> GetGroupByIdAsync(long id)
        {
            const string sqlGroup = "SELECT \"GroupId\", \"GroupName\" FROM \"Group\" WHERE \"GroupId\" = @Id";

            const string sqlAssociations = "SELECT \"UsuarioId\" FROM \"UsuarioGroup\" WHERE \"GroupId\" = @Id";

            using (var connection = _connection.CreateConnection())
            {
                var group = await connection.QueryFirstOrDefaultAsync<Group>(sqlGroup, new { Id = id });

                if (group == null)
                    return null;

                var usuarioIds = await connection.QueryAsync<long>(sqlAssociations, new { Id = id });

                group.UsuariosIds = usuarioIds.ToList();

                return group;
            }
        }

        public async Task<IEnumerable<Group>> GetGroupsByUserIdAsync(long userId)
        {
            const string query = @"
                SELECT g.""GroupId"", g.""GroupName"" 
                FROM ""Group"" g
                INNER JOIN ""UsuarioGroup"" ug ON g.""GroupId"" = ug.""GroupId""
                WHERE ug.""UsuarioId"" = @UserId";

            const string assocQuery = @"
                SELECT ""UsuarioId"" 
                FROM ""UsuarioGroup"" 
                WHERE ""GroupId"" = @GroupId";

            using (var connection = _connection.CreateConnection())
            {
                var groups = await connection.QueryAsync<Group>(query, new { UserId = userId });

                foreach (var group in groups)
                {
                    var usuarioIds = await connection.QueryAsync<long>(assocQuery, new { GroupId = group.GroupId });
                    group.UsuariosIds = usuarioIds.ToList();
                }

                return groups;
            }
        }

        public async Task<Group> GetGroupByNameAsync(string groupName)
        {
            const string query = "SELECT * FROM \"Group\" WHERE \"GroupName\" ILIKE @GroupName";

            const string sqlAssociations = "SELECT \"UsuarioId\" FROM \"UsuarioGroup\" WHERE \"GroupId\" = @Id";

            using (var connection = _connection.CreateConnection())
            {
                var group = await connection.QueryFirstOrDefaultAsync<Group>(query, new { GroupName = groupName });

                if (group == null)
                    return null;

                var usuarioIds = await connection.QueryAsync<long>(sqlAssociations, new { Id = group.GroupId });

                group.UsuariosIds = usuarioIds.ToList();

                return group;
            }
        }

        public async Task<Group> AddUsersToGroupAsync(long groupId, List<long> userIds)
        {
            if (userIds == null || userIds.Count == 0)
                return await GetGroupByIdAsync(groupId);

            var distinctUserIds = userIds.Distinct().ToList();

            using var connection = _connection.CreateConnection();
            if (connection.State == System.Data.ConnectionState.Closed)
                connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                const string selectQuery = @"
                    SELECT ""UsuarioId""
                    FROM ""UsuarioGroup""
                    WHERE ""GroupId"" = @GroupId AND ""UsuarioId"" = ANY(@UserIds)";

                var existing = (await connection.QueryAsync<long>(selectQuery, new { GroupId = groupId, UserIds = distinctUserIds }, transaction)).ToHashSet();

                var toInsert = distinctUserIds.Where(id => !existing.Contains(id)).ToList();

                const string insertQuery = @"
                    INSERT INTO ""UsuarioGroup"" (""UsuarioId"", ""GroupId"")
                    VALUES (@UsuarioId, @GroupId);";

                foreach (var usuarioId in toInsert)
                {
                    await connection.ExecuteAsync(insertQuery, new { UsuarioId = usuarioId, GroupId = groupId }, transaction);
                }

                transaction.Commit();

                return await GetGroupByIdAsync(groupId);
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<Group> RemoveUsersFromGroupAsync(long groupId, List<long> userIds)
        {
            const string query = @"
                DELETE FROM ""UsuarioGroup""
                WHERE ""UsuarioId"" = @UserId AND ""GroupId"" = @GroupId;";


            using (var connection = _connection.CreateConnection())
            {
                foreach (var userId in userIds)
                {
                    await connection.ExecuteAsync(query, new { UserId = userId, GroupId = groupId });
                }

                return await GetGroupByIdAsync(groupId);
            }
        }

        public async Task<Group> CreateGroupAsync(Group group)
        {
            const string query = @"
                INSERT INTO ""Group"" (""GroupName"") -- Aspas duplas na tabela e coluna
                VALUES (@GroupName)
                RETURNING ""GroupId"";";

            using (var connection = _connection.CreateConnection())
            {
                var groupId = await connection.QuerySingleAsync<int>(query, new { group.GroupName });

                await AddUsersToGroupAsync(groupId, group.UsuariosIds);

                return await GetGroupByIdAsync(groupId);
            }
        }

        public async Task<bool> UpdateGroupAsync(Group group)
        {
            const string query = @"
                UPDATE ""Group"" -- Aspas duplas na tabela
                SET ""GroupName"" = @GroupName
                WHERE ""GroupId"" = @GroupId;";


            using (var connection = _connection.CreateConnection())
            {
                var affectedRows = await connection.ExecuteAsync(query, new { group.GroupName, group.GroupId });
                return affectedRows > 0;
            }
        }

        public async Task<bool> DeleteGroupAsync(long id)
        {
            const string query = "DELETE FROM \"Group\" WHERE \"GroupId\" = @Id;";

            using (var connection = _connection.CreateConnection())
            {
                var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
                return affectedRows > 0;
            }
        }
    }
}
