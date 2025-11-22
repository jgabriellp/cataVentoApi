using CataVentoApi.DataContext;
using CataVentoApi.Entity;
using CataVentoApi.Entity.Dto.RequestDto;
using CataVentoApi.Entity.Dto.ResponseDto;
using CataVentoApi.Repositories.Interface;
using Dapper;

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
                    SELECT GroupId, GroupName FROM [Group] 
                    ORDER BY {orderByField} ASC 
                    OFFSET @Offset ROWS 
                    FETCH NEXT @PageSize ROWS ONLY;
                ";

                var groups = (await connection.QueryAsync<Group>(sqlGroupsPaged, parameters)).ToList();

                //if (!groups.Any()) return Enumerable.Empty<Group>();

                var groupIds = groups.Select(g => g.GroupId);

                const string sqlAssociationsFiltered = @"
                    SELECT UsuarioId, GroupId FROM UsuarioGroup
                    WHERE GroupId IN @GroupIds;
                ";

                var associationParameters = new { GroupIds = groupIds };

                var associations = await connection.QueryAsync<
                     (long UsuarioId, long GroupId)>(sqlAssociationsFiltered, associationParameters);

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
            const string sqlGroups = "SELECT GroupId, GroupName FROM [Group]";
            const string sqlAssociations = "SELECT UsuarioId, GroupId FROM UsuarioGroup";

            using (var connection = _connection.CreateConnection())
            {
                // busca todos os grupos
                var groups = (await connection.QueryAsync<Group>(sqlGroups)).ToList();

                // retorna uma tupla com os IDs de Usuário e GroupId
                var associations = await connection.QueryAsync<
                    (long UsuarioId, long GroupId)>(sqlAssociations);

                // agrupa por GroupId e, para cada grupo, cria uma lista de IDs de Usuário
                var groupedAssociations = associations
                    .GroupBy(a => a.GroupId)
                    .ToDictionary(g => g.Key, g => g.Select(a => a.UsuarioId).ToList());

                // cada grupo recebe sua lista de IDs de Usuário
                foreach (var group in groups)
                {
                    // Se o GroupId estiver no dicionário de associações
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
            const string sqlGroup = "SELECT GroupId, GroupName FROM [Group] WHERE GroupId = @Id";
            const string sqlAssociations = "SELECT UsuarioId FROM UsuarioGroup WHERE GroupId = @Id";

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
                SELECT g.* 
                FROM [Group] g
                INNER JOIN UsuarioGroup ug ON g.GroupId = ug.GroupId
                WHERE ug.UsuarioId = @UserId";
            
            const string assocQuery = @"
                SELECT UsuarioId 
                FROM UsuarioGroup 
                WHERE GroupId = @GroupId";

            using (var connection = _connection.CreateConnection())
            {
                var groups = await connection.QueryAsync<Group>(query, new { UserId = userId });
                
                foreach(var group in groups)
                {
                    var usuarioIds = await connection.QueryAsync<long>(assocQuery, new { GroupId = group.GroupId });
                    group.UsuariosIds = usuarioIds.ToList();
                }

                return groups;
            }
        }

        public async Task<Group> GetGroupByNameAsync(string groupName)
        {
            const string query = "SELECT * FROM [Group] WHERE GroupName = @GroupName";
            const string sqlAssociations = "SELECT UsuarioId FROM UsuarioGroup WHERE GroupId = @Id";

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

            // garantir IDs únicos
            var distinctUserIds = userIds.Distinct().ToList();

            using var connection = _connection.CreateConnection();
            if (connection.State == System.Data.ConnectionState.Closed)
                connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // buscar usuários já vinculados
                const string selectQuery = @"
                SELECT UsuarioId
                FROM UsuarioGroup
                WHERE GroupId = @GroupId AND UsuarioId IN @UserIds";
                var existing = (await connection.QueryAsync<long>(selectQuery, new { GroupId = groupId, UserIds = distinctUserIds }, transaction)).ToHashSet();

                var toInsert = distinctUserIds.Where(id => !existing.Contains(id)).ToList();

                const string insertQuery = @"
                INSERT INTO UsuarioGroup (UsuarioId, GroupId)
                VALUES (@UsuarioId, @GroupId);";

                foreach (var usuarioId in toInsert)
                {
                    await connection.ExecuteAsync(insertQuery, new { UsuarioId = usuarioId, GroupId = groupId }, transaction);
                }

                transaction.Commit();

                // retornar grupo atualizado (assumindo que GetGroupByIdAsync lê relacionamentos)
                return await GetGroupByIdAsync(groupId);
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            //const string query = @"
            //    INSERT INTO UsuarioGroup (UsuarioId, GroupId)
            //    VALUES (@UserId, @GroupId);";
            //using(var connection = _connection.CreateConnection())
            //{
            //    foreach(var userId in userIds)
            //    {
            //        await connection.ExecuteAsync(query, new { UserId = userId, GroupId = groupId });
            //    }
            //    return await GetGroupByIdAsync(groupId);
            //}
        }

        public async Task<Group> RemoveUsersFromGroupAsync(long groupId, List<long> userIds)
        {
            const string query = @"
                DELETE FROM UsuarioGroup
                WHERE UsuarioId = @UserId AND GroupId = @GroupId;";
            using(var connection = _connection.CreateConnection())
            {
                foreach(var userId in userIds)
                {
                    await connection.ExecuteAsync(query, new { UserId = userId, GroupId = groupId });
                }
                return await GetGroupByIdAsync(groupId);
            }
        }

        public async Task<Group> CreateGroupAsync(Group group)
        {
            const string query = @"
                INSERT INTO [Group] (GroupName)
                VALUES (@GroupName);
                SELECT CAST(SCOPE_IDENTITY() as bigint);";
            using(var connection = _connection.CreateConnection())
            {
                var groupId = await connection.QuerySingleAsync<long>(query, new { group.GroupName });
                await AddUsersToGroupAsync(groupId, group.UsuariosIds);
                return await GetGroupByIdAsync(groupId);
            }
        }

        public async Task<bool> UpdateGroupAsync(Group group)
        {
            const string query = @"
                UPDATE [Group]
                SET GroupName = @GroupName
                WHERE GroupId = @GroupId;";
            using(var connection = _connection.CreateConnection())
            {
                var affectedRows = await connection.ExecuteAsync(query, new { group.GroupName, group.GroupId });
                return affectedRows > 0;
            }
        }

        public async Task<bool> DeleteGroupAsync(long id)
        {
            const string query = "DELETE FROM [Group] WHERE GroupId = @Id;";
            using (var connection = _connection.CreateConnection())
            {
                var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
                return affectedRows > 0;
            }
        }
    }
}
