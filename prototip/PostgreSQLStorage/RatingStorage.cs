using Core.Model;
using Core.Storage;
using Npgsql;

namespace PostgreSQLStorage
{
    public class RatingStorage(Database database) : IRatingStorage
    {
        private readonly Database _db = database;

        public async Task<Rating?> GetById(string ratingId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                "SELECT * FROM ratings WHERE ratingId = @id", conn);
            cmd.Parameters.AddWithValue("id", ratingId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Rating(
                    reader.GetString(reader.GetOrdinal("ratingId")),
                    reader.GetDecimal(reader.GetOrdinal("value")),
                    reader.GetDateTime(reader.GetOrdinal("ratingDate")),
                    reader.GetString(reader.GetOrdinal("userId")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("workId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("workId")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("authorId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("authorId"))
                );
            }
            return null;
        }

        public async Task<List<Rating>> GetByAuthorId(string authorId)
        {
            var ratings = new List<Rating>();
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                "SELECT * FROM ratings WHERE authorId = @authorId ORDER BY ratingDate DESC", conn);
            cmd.Parameters.AddWithValue("authorId", authorId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                ratings.Add(new Rating(
                    reader.GetString(reader.GetOrdinal("ratingId")),
                    reader.GetDecimal(reader.GetOrdinal("value")),
                    reader.GetDateTime(reader.GetOrdinal("ratingDate")),
                    reader.GetString(reader.GetOrdinal("userId")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("workId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("workId")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("authorId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("authorId"))
                ));
            }
            return ratings;
        }

        public async Task<List<Rating>> GetByWorkId(string workId)
        {
            var ratings = new List<Rating>();
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                "SELECT * FROM ratings WHERE workId = @workId ORDER BY ratingDate DESC", conn);
            cmd.Parameters.AddWithValue("workId", workId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                ratings.Add(new Rating(
                    reader.GetString(reader.GetOrdinal("ratingId")),
                    reader.GetDecimal(reader.GetOrdinal("value")),
                    reader.GetDateTime(reader.GetOrdinal("ratingDate")),
                    reader.GetString(reader.GetOrdinal("userId")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("workId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("workId")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("authorId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("authorId"))
                ));
            }
            return ratings;
        }

        public async Task<Rating?> GetUserRatingForAuthor(string userId, string authorId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                "SELECT * FROM ratings WHERE userId = @userId AND authorId = @authorId", conn);
            cmd.Parameters.AddWithValue("userId", userId);
            cmd.Parameters.AddWithValue("authorId", authorId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Rating(
                    reader.GetString(reader.GetOrdinal("ratingId")),
                    reader.GetDecimal(reader.GetOrdinal("value")),
                    reader.GetDateTime(reader.GetOrdinal("ratingDate")),
                    reader.GetString(reader.GetOrdinal("userId")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("workId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("workId")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("authorId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("authorId"))
                );
            }
            return null;
        }

        public async Task<Rating?> GetUserRatingForWork(string userId, string workId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                "SELECT * FROM ratings WHERE userId = @userId AND workId = @workId", conn);
            cmd.Parameters.AddWithValue("userId", userId);
            cmd.Parameters.AddWithValue("workId", workId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Rating(
                    reader.GetString(reader.GetOrdinal("ratingId")),
                    reader.GetDecimal(reader.GetOrdinal("value")),
                    reader.GetDateTime(reader.GetOrdinal("ratingDate")),
                    reader.GetString(reader.GetOrdinal("userId")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("workId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("workId")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("authorId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("authorId"))
                );
            }
            return null;
        }

        public async Task<decimal?> GetAverageRatingForAuthor(string authorId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                "SELECT AVG(value) FROM ratings WHERE authorId = @authorId", conn);
            cmd.Parameters.AddWithValue("authorId", authorId);

            var result = await cmd.ExecuteScalarAsync();
            return result == DBNull.Value ? null : Convert.ToDecimal(result);
        }

        public async Task<decimal?> GetAverageRatingForWork(string workId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                "SELECT AVG(value) FROM ratings WHERE workId = @workId", conn);
            cmd.Parameters.AddWithValue("workId", workId);

            var result = await cmd.ExecuteScalarAsync();
            return result == DBNull.Value ? null : Convert.ToDecimal(result);
        }

        public async Task<Rating?> GetEditorRatingForAuthor(string authorId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT r.* FROM ratings r
                  INNER JOIN users u ON r.userId = u.userId
                  WHERE r.authorId = @authorId AND u.role = @editorRole
                  ORDER BY r.ratingDate DESC
                  LIMIT 1", conn);
            cmd.Parameters.AddWithValue("authorId", authorId);
            cmd.Parameters.AddWithValue("editorRole", UserRole.EDITOR);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Rating(
                    reader.GetString(reader.GetOrdinal("ratingId")),
                    reader.GetDecimal(reader.GetOrdinal("value")),
                    reader.GetDateTime(reader.GetOrdinal("ratingDate")),
                    reader.GetString(reader.GetOrdinal("userId")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("workId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("workId")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("authorId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("authorId"))
                );
            }
            return null;
        }

        public async Task<Rating?> GetEditorRatingForWork(string workId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT r.* FROM ratings r
                  INNER JOIN users u ON r.userId = u.userId
                  WHERE r.workId = @workId AND u.role = @editorRole
                  ORDER BY r.ratingDate DESC
                  LIMIT 1", conn);
            cmd.Parameters.AddWithValue("workId", workId);
            cmd.Parameters.AddWithValue("editorRole", UserRole.EDITOR);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Rating(
                    reader.GetString(reader.GetOrdinal("ratingId")),
                    reader.GetDecimal(reader.GetOrdinal("value")),
                    reader.GetDateTime(reader.GetOrdinal("ratingDate")),
                    reader.GetString(reader.GetOrdinal("userId")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("workId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("workId")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("authorId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("authorId"))
                );
            }
            return null;
        }

        public async System.Threading.Tasks.Task Add(Rating rating)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"INSERT INTO ratings (ratingId, value, ratingDate, userId, workId, authorId)
                  VALUES (@id, @value, @date, @userId, @workId, @authorId)", conn);
            cmd.Parameters.AddWithValue("id", rating.RatingId);
            cmd.Parameters.AddWithValue("value", rating.Value);
            cmd.Parameters.AddWithValue("date", rating.RatingDate);
            cmd.Parameters.AddWithValue("userId", rating.UserId);
            cmd.Parameters.AddWithValue("workId", (object?)rating.WorkId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("authorId", (object?)rating.AuthorId ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }

        public async System.Threading.Tasks.Task Update(Rating rating)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"UPDATE ratings 
                  SET value = @value, ratingDate = @date
                  WHERE ratingId = @id", conn);
            cmd.Parameters.AddWithValue("id", rating.RatingId);
            cmd.Parameters.AddWithValue("value", rating.Value);
            cmd.Parameters.AddWithValue("date", rating.RatingDate);

            await cmd.ExecuteNonQueryAsync();
        }

        public async System.Threading.Tasks.Task Delete(string ratingId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                "DELETE FROM ratings WHERE ratingId = @id", conn);
            cmd.Parameters.AddWithValue("id", ratingId);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
