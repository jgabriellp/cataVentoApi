using CataVentoApi.DataContext;
using CataVentoApi.Entity;
using CataVentoApi.Repositories.Interface;
using Dapper;

namespace CataVentoApi.Repositories.Repository
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly DapperContext _connection;

        public UsuarioRepository(DapperContext connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<Usuario>> GetAll(int pageNumber, int pageSize)
        {
            const string orderByField = "Id";

            int offset = (pageNumber - 1) * pageSize;

            using (var connection = _connection.CreateConnection())
            {
                var parameters = new { Offset = offset, PageSize = pageSize };

                var sqlUsersPaged = $@"
                    SELECT ""Id"", ""Name"", ""LastName"", ""Role"", ""Email"", ""Password"", ""PhotoUrl"" 
                    FROM ""Usuario"" 
                    ORDER BY ""{orderByField}"" ASC  -- Usamos ""{orderByField}""
                    LIMIT @PageSize OFFSET @Offset;  -- MUDANÇA CRÍTICA para PostgreSQL
                ";

                var users = (await connection.QueryAsync<Usuario>(sqlUsersPaged, parameters)).ToList();

                if (!users.Any()) return Enumerable.Empty<Usuario>();

                var userIds = users.Select(u => u.Id).ToList();

                const string sqlAssociationsFiltered = @"
                    SELECT ""UsuarioId"", ""GroupId"" FROM ""UsuarioGroup""
                    WHERE ""UsuarioId"" = ANY(@UserIds); -- PostgreSQL prefere = ANY() para listas de parâmetros
                ";

                var associationParameters = new { UserIds = userIds };

                var associations = await connection.QueryAsync<
                     (long UsuarioId, int GroupId)>(sqlAssociationsFiltered, associationParameters);
                     // Usamos 'int' para GroupId, pois "GroupId" é SERIAL/INT.

                var groupedAssociations = associations
                    .ToLookup(a => a.UsuarioId, a => a.GroupId);

                foreach (var user in users)
                {
                    // O tipo GroupIds na entidade Usuario deve ser List<int>
                    user.GroupIds = groupedAssociations[user.Id].ToList();
                }

                return users;
            }
        }

        public async Task<IEnumerable<Usuario>> GetAll()
        {
            const string query = "SELECT * FROM \"Usuario\"";

            const string queryAssociations = "SELECT \"UsuarioId\", \"GroupId\" FROM \"UsuarioGroup\"";

            using (var connection = _connection.CreateConnection())
            {
                var users = await connection.QueryAsync<Usuario>(query);

                var associations = await connection.QueryAsync<
                    (long UsuarioId, int GroupId)>(queryAssociations);

                var groupedAssociations = associations
                    .GroupBy(a => a.UsuarioId)
                    .ToDictionary(g => g.Key, g => g.Select(a => a.GroupId).ToList());

                foreach (var user in users)
                {
                    if (groupedAssociations.TryGetValue(user.Id, out var groupIds))
                    {
                        user.GroupIds = groupIds;
                    }
                }

                return users;
            }
        }

        public async Task<IEnumerable<Usuario>> GetAllUsersByGroupIdAsync(long groupId)
        {
            const string query = @"
                SELECT u.""Id"", u.""Name"", u.""LastName"", u.""Role"", u.""Email"", u.""Password"", u.""PhotoUrl"" -- Seleção explícita de colunas com ""
                FROM ""Usuario"" u
                INNER JOIN ""UsuarioGroup"" ug ON u.""Id"" = ug.""UsuarioId""
                WHERE ug.""GroupId"" = @GroupId";

            const string queryAssociations = @"
                SELECT ""GroupId"" -- Seleciona apenas o GroupId, que é um INT
                FROM ""UsuarioGroup""
                WHERE ""UsuarioId"" = @UsuarioId";

            using (var connection = _connection.CreateConnection())
            {
                var users = await connection.QueryAsync<Usuario>(query, new { GroupId = groupId });

                foreach (var user in users)
                {
                    var groupsIds = await connection.QueryAsync<int>(queryAssociations, new { UsuarioId = user.Id });
                    user.GroupIds = groupsIds.ToList();
                }

                return users;
            }
        }

        public async Task<IEnumerable<Usuario>> GetByName(string name)
        {
            const string query = "SELECT * FROM \"Usuario\" WHERE \"Name\" ILIKE @NamePattern";

            using (var connection = _connection.CreateConnection())
            {
                var users = (await connection.QueryAsync<Usuario>(
                    query,
                    new { NamePattern = $"%{name}%" }
                )).ToList();

                if (!users.Any())
                    return Enumerable.Empty<Usuario>();

                var userIds = users.Select(u => u.Id).ToList();

                const string queryAssociations = @"
                    SELECT ""UsuarioId"", ""GroupId"" 
                    FROM ""UsuarioGroup"" 
                    WHERE ""UsuarioId"" = ANY(@UserIds)";

                var associations = await connection.QueryAsync<
                    (long UsuarioId, int GroupId)>(
                        queryAssociations,
                        new { UserIds = userIds }
                    );

                var groupedAssociations = associations
                    .ToLookup(a => a.UsuarioId, a => a.GroupId);

                foreach (var user in users)
                {
                    user.GroupIds = groupedAssociations[user.Id].ToList();
                }

                return users;
            }
        }

        public async Task<Usuario> GetByEmail(string email)
        {
            const string query = "SELECT * FROM \"Usuario\" WHERE \"Email\" = @Email";

            const string queryAssociations = "SELECT \"GroupId\" FROM \"UsuarioGroup\" WHERE \"UsuarioId\" = @UsuarioId";

            using (var connection = _connection.CreateConnection())
            {
                var user = await connection.QueryFirstOrDefaultAsync<Usuario>(query, new { Email = email });

                if (user == null)
                    return null;

                var groupsId = await connection.QueryAsync<int>(queryAssociations, new { UsuarioId = user.Id });

                user.GroupIds = groupsId.ToList();

                return user;
            }
        }

        public async Task<Usuario> GetById(long id)
        {
            const string query = "SELECT * FROM \"Usuario\" WHERE \"Id\" = @Id";

            const string queryAssociations = "SELECT \"GroupId\" FROM \"UsuarioGroup\" WHERE \"UsuarioId\" = @UsuarioId";

            using (var connection = _connection.CreateConnection())
            {
                var user = await connection.QueryFirstOrDefaultAsync<Usuario>(query, new { Id = id });
                if (user == null)
                    return null;

                var groupsId = await connection.QueryAsync<int>(queryAssociations, new { UsuarioId = user.Id });

                user.GroupIds = groupsId.ToList();

                return user;
            }
        }

        public async Task<Usuario> CreateAsync(Usuario usuario)
        {
            const string query = @"
                INSERT INTO ""Usuario"" (""Name"", ""LastName"", ""Role"", ""Email"", ""Password"", ""PhotoUrl"")
                VALUES (@Name, @LastName, @Role, @Email, @Password, @PhotoUrl)
                RETURNING ""Id"";";

            using (var connection = _connection.CreateConnection())
            {
                var id = await connection.ExecuteScalarAsync<long>(query, usuario);
                usuario.Id = id;
                return usuario;
            }
        }

        public async Task<bool> UpdateAsync(Usuario usuario)
        {
            const string query = @"
                UPDATE ""Usuario""
                SET ""Name"" = @Name,
                    ""LastName"" = @LastName,
                    ""Role"" = @Role,
                    ""Email"" = @Email,
                    ""Password"" = @Password,
                    ""PhotoUrl"" = @PhotoUrl
                WHERE ""Id"" = @Id";

            using (var connection = _connection.CreateConnection())
            {
                var affectedRows = await connection.ExecuteAsync(query, usuario);
                return affectedRows > 0;
            }
        }

        public async Task<bool> DeleteAsync(long id)
        {
            const string query = "DELETE FROM \"Usuario\" WHERE \"Id\" = @Id";

            using (var connection = _connection.CreateConnection())
            {
                var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
                return affectedRows > 0;
            }
        }
    }
}
