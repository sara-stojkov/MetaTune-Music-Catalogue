using Core.Model;
using Core.Storage;
using Npgsql;
using Task = System.Threading.Tasks.Task;

namespace PostgreSQLStorage
{
    public class TaskStorage : ITaskStorage
    {
        private readonly Database _db;

        public TaskStorage(Database db)
        {
            _db = db;
        }

        public async Task<Core.Model.Task?> GetById(string id)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT taskId, assignmentDate, done, userId, workId, authorId 
                  FROM tasks 
                  WHERE taskId = @id", conn);

            cmd.Parameters.AddWithValue("id", id);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var taskId = reader.GetString(0);
                var assignmentDate = reader.GetDateTime(1);
                var done = reader.GetBoolean(2);
                var userId = reader.GetString(3);
                var workId = await reader.IsDBNullAsync(4) ? null : reader.GetString(4);
                var authorId = await reader.IsDBNullAsync(5) ? null : reader.GetString(5);

                return new Core.Model.Task(taskId, assignmentDate, done, userId, workId, authorId);
            }

            return null;
        }

        public async Task<List<Core.Model.Task>> GetAll()
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT taskId, assignmentDate, done, userId, workId, authorId 
                  FROM tasks 
                  ORDER BY assignmentDate DESC", conn);

            using var reader = await cmd.ExecuteReaderAsync();

            var tasks = new List<Core.Model.Task>();

            while (await reader.ReadAsync())
            {
                var taskId = reader.GetString(0);
                var assignmentDate = reader.GetDateTime(1);
                var done = reader.GetBoolean(2);
                var userId = reader.GetString(3);
                var workId = await reader.IsDBNullAsync(4) ? null : reader.GetString(4);
                var authorId = await reader.IsDBNullAsync(5) ? null : reader.GetString(5);

                tasks.Add(new Core.Model.Task(taskId, assignmentDate, done, userId, workId, authorId));
            }

            return tasks;
        }

        public async Task CreateOne(Core.Model.Task task)
        {
            using var conn = _db.GetConnection();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                string sql = @"INSERT INTO tasks(taskId, assignmentDate, done, userId, workId, authorId) 
                               VALUES(@taskId, @assignmentDate, @done, @userId, @workId, @authorId)";

                using var cmd = new NpgsqlCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("taskId", task.TaskId);
                cmd.Parameters.AddWithValue("assignmentDate", task.AssignmentDate);
                cmd.Parameters.AddWithValue("done", task.Done);
                cmd.Parameters.AddWithValue("userId", task.UserId);
                cmd.Parameters.AddWithValue("workId", (object?)task.WorkId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("authorId", (object?)task.AuthorId ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateOne(Core.Model.Task task)
        {
            using var conn = _db.GetConnection();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                string sql = @"UPDATE tasks 
                               SET assignmentDate = @assignmentDate, 
                                   done = @done, 
                                   userId = @userId, 
                                   workId = @workId, 
                                   authorId = @authorId 
                               WHERE taskId = @taskId";

                using var cmd = new NpgsqlCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("taskId", task.TaskId);
                cmd.Parameters.AddWithValue("assignmentDate", task.AssignmentDate);
                cmd.Parameters.AddWithValue("done", task.Done);
                cmd.Parameters.AddWithValue("userId", task.UserId);
                cmd.Parameters.AddWithValue("workId", (object?)task.WorkId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("authorId", (object?)task.AuthorId ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteById(string id)
        {
            using var conn = _db.GetConnection();
            string sql = @"DELETE FROM tasks WHERE taskId = @id";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<Core.Model.Task>> GetAllByEditorId(string userId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT taskId, assignmentDate, done, userId, workId, authorId 
                  FROM tasks 
                  WHERE userId = @userId
                  ORDER BY assignmentDate DESC", conn);

            cmd.Parameters.AddWithValue("userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();

            var tasks = new List<Core.Model.Task>();

            while (await reader.ReadAsync())
            {
                var taskId = reader.GetString(0);
                var assignmentDate = reader.GetDateTime(1);
                var done = reader.GetBoolean(2);
                var userIdResult = reader.GetString(3);
                var workId = await reader.IsDBNullAsync(4) ? null : reader.GetString(4);
                var authorId = await reader.IsDBNullAsync(5) ? null : reader.GetString(5);

                tasks.Add(new Core.Model.Task(taskId, assignmentDate, done, userIdResult, workId, authorId));
            }

            return tasks;
        }
    }
}