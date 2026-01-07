using CataVentoApi.DataContext;
using CataVentoApi.Entity;
using CataVentoApi.Entity.Dto.RequestDto;
using CataVentoApi.Entity.Enums;
using CataVentoApi.Repositories.Interface;
using Dapper;

namespace CataVentoApi.Repositories.Repository
{
    public class TaskRepository : ITaskRepository
    {
        private readonly DapperContext _connection;

        public TaskRepository(DapperContext connection)
        {
            _connection = connection;
        }

        // 1. CREATE - Com tratamento para erro de ExecuteScalar e tipos do Postgres
        public async Task<int> CreateTaskAsync(KanbanTask task)
        {
            using (var connection = _connection.CreateConnection())
            {
                // 1. Garantimos que o Status seja tratado como INT explicitamente para a query de posição
                const string sqlPos = @"SELECT COALESCE(MAX(position), 0) + 1 FROM tasks WHERE status = @Status";

                // Usamos dynamic para evitar problemas de cast direto do Postgres (que retorna long) para int
                var nextPosition = await connection.ExecuteScalarAsync<object>(sqlPos, new { Status = (int)task.Status });
                task.Position = Convert.ToInt32(nextPosition);

                // 2. Query de Insert
                // Dica: Certifique-se que os nomes das propriedades em 'task' batem exatamente com @Title, @Description, etc.
                const string sql = @"
                    INSERT INTO tasks (title, description, usuario_id, due_date, priority, status, position)
                    VALUES (@Title, @Description, @UsuarioId, @DueDate, @Priority, @Status, @Position)
                    RETURNING id;";

                // O Postgres retorna o ID como Int32 ou Int64 dependendo do SERIAL. Convert.ToInt32 é o mais seguro.
                var idGerado = await connection.ExecuteScalarAsync<object>(sql, new
                {
                    task.Title,
                    task.Description,
                    task.UsuarioId,
                    task.DueDate,
                    Priority = (int)task.Priority, // Cast explícito para evitar erro de tipo
                    Status = (int)task.Status,     // Cast explícito para evitar erro de tipo
                    task.Position
                });

                return Convert.ToInt32(idGerado);
            }
        }

        public async Task<KanbanTask?> GetTaskByIdAsync(int id)
        {
            const string query = @"
                SELECT t.*, u.""Id"", u.""Name"", u.""PhotoUrl""
                FROM tasks t
                LEFT JOIN ""Usuario"" u ON t.usuario_id = u.""Id""
                WHERE t.id = @id";

            using (var connection = _connection.CreateConnection())
            {
                var result = await connection.QueryAsync<KanbanTask, UsuarioResponsavel, KanbanTask>(
                    query,
                    (task, usuario) =>
                    {
                        task.Responsavel = usuario;
                        return task;
                    },
                    new { id },
                    splitOn: "Id"
                );

                return result.FirstOrDefault();
            }
        }

        // 2. READ - Trazendo os dados do Usuário Responsável
        public async Task<IEnumerable<KanbanTask>> GetAllTasksAsync()
        {
            using (var connection = _connection.CreateConnection())
            {
                const string sql = @"
                    SELECT t.*, u.""Id"", u.""Name"", u.""PhotoUrl""
                    FROM tasks t
                    LEFT JOIN ""Usuario"" u ON t.usuario_id = u.""Id""
                    ORDER BY t.status, t.position";

                return await connection.QueryAsync<KanbanTask, UsuarioResponsavel, KanbanTask>(
                    sql,
                    (task, usuario) =>
                    {
                        task.Responsavel = usuario;
                        return task;
                    },
                    splitOn: "Id"
                );
            }
        }

        // 3. UPDATE - Atualização geral de campos
        public async Task<bool> UpdateTaskAsync(KanbanTask task)
        {
            using (var connection = _connection.CreateConnection())
            {
                const string sql = @"
                    UPDATE tasks 
                    SET title = @Title, 
                        description = @Description, 
                        usuario_id = @UsuarioId, 
                        due_date = @DueDate, 
                        priority = @Priority 
                    WHERE id = @Id";

                var rows = await connection.ExecuteAsync(sql, new
                {
                    task.Title,
                    task.Description,
                    task.UsuarioId,
                    task.DueDate,
                    Priority = (int)task.Priority,
                    task.Id
                });
                return rows > 0;
            }
        }

        // 4. UPDATE COLUMN - Mover card entre colunas
        public async Task<bool> UpdateColumnAsync(int id, KanbanTaskStatusEnum newStatus, int newPosition)
        {
            using (var connection = _connection.CreateConnection())
            {
                const string sql = @"
                    UPDATE tasks 
                    SET status = @newStatus, 
                        position = @newPosition 
                    WHERE id = @id";

                var rows = await connection.ExecuteAsync(sql, new
                {
                    id,
                    newStatus = (int)newStatus,
                    newPosition
                });
                return rows > 0;
            }
        }

        // 5. REORDER - Atualização em lote (Otimizada para o seu Front-end)
        public async Task<bool> ReorderTasksAsync(List<UpdatePriorityRequestDto> tasks)
        {
            using (var connection = _connection.CreateConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        const string sql = @"
                            UPDATE tasks 
                            SET position = @Position, 
                                status = @Status 
                            WHERE id = @Id";

                        // O Dapper mapeia automaticamente a lista de DTOs para os parâmetros da query
                        await connection.ExecuteAsync(sql, tasks, transaction: transaction);

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        // 6. DELETE
        public async Task<bool> DeleteTaskAsync(int id)
        {
            using (var connection = _connection.CreateConnection())
            {
                const string sql = "DELETE FROM tasks WHERE id = @id";
                var rows = await connection.ExecuteAsync(sql, new { id });
                return rows > 0;
            }
        }
    }
}
