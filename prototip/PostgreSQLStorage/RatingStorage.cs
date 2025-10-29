using Core.Model;
using Core.Storage;
using Npgsql;
using Task = System.Threading.Tasks.Task;

namespace PostgreSQLStorage
{
    public class RatingStorage : IRatingStorage
    {
        private readonly Database _db;

        public RatingStorage(Database db)
        {
            _db = db;
        }

        public async Task<Rating?> GetById(string ratingId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT ratingId, value, ratingDate, userId, workId, authorId 
                  FROM ratings 
                  WHERE ratingId = @ratingId", conn);

            cmd.Parameters.AddWithValue("ratingId", ratingId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var id = reader.GetString(0);
                var value = reader.GetDecimal(1);
                var ratingDate = reader.GetDateTime(2);
                var userId = reader.GetString(3);
                var workId = await reader.IsDBNullAsync(4) ? null : reader.GetString(4);
                var authorId = await reader.IsDBNullAsync(5) ? null : reader.GetString(5);

                return new Rating(id, value, ratingDate, userId, workId, authorId);
            }

            return null;
        }

        public async Task<List<Rating>> GetAll()
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT ratingId, value, ratingDate, userId, workId, authorId 
                  FROM ratings 
                  ORDER BY ratingDate DESC", conn);

            using var reader = await cmd.ExecuteReaderAsync();

            var ratings = new List<Rating>();

            while (await reader.ReadAsync())
            {
                var ratingId = reader.GetString(0);
                var value = reader.GetDecimal(1);
                var ratingDate = reader.GetDateTime(2);
                var userId = reader.GetString(3);
                var workId = await reader.IsDBNullAsync(4) ? null : reader.GetString(4);
                var authorId = await reader.IsDBNullAsync(5) ? null : reader.GetString(5);

                ratings.Add(new Rating(ratingId, value, ratingDate, userId, workId, authorId));
            }

            return ratings;
        }

        public async Task<List<Rating>> GetAllByUserId(string userId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT ratingId, value, ratingDate, userId, workId, authorId 
                  FROM ratings 
                  WHERE userId = @userId
                  ORDER BY ratingDate DESC", conn);

            cmd.Parameters.AddWithValue("userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();

            var ratings = new List<Rating>();

            while (await reader.ReadAsync())
            {
                var ratingId = reader.GetString(0);
                var value = reader.GetDecimal(1);
                var ratingDate = reader.GetDateTime(2);
                var userIdResult = reader.GetString(3);
                var workId = await reader.IsDBNullAsync(4) ? null : reader.GetString(4);
                var authorId = await reader.IsDBNullAsync(5) ? null : reader.GetString(5);

                ratings.Add(new Rating(ratingId, value, ratingDate, userIdResult, workId, authorId));
            }

            return ratings;
        }

        public async Task<List<Rating>> GetAllByWorkId(string workId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT ratingId, value, ratingDate, userId, workId, authorId 
                  FROM ratings 
                  WHERE workId = @workId
                  ORDER BY ratingDate DESC", conn);

            cmd.Parameters.AddWithValue("workId", workId);

            using var reader = await cmd.ExecuteReaderAsync();

            var ratings = new List<Rating>();

            while (await reader.ReadAsync())
            {
                var ratingId = reader.GetString(0);
                var value = reader.GetDecimal(1);
                var ratingDate = reader.GetDateTime(2);
                var userId = reader.GetString(3);
                var workIdResult = await reader.IsDBNullAsync(4) ? null : reader.GetString(4);
                var authorId = await reader.IsDBNullAsync(5) ? null : reader.GetString(5);

                ratings.Add(new Rating(ratingId, value, ratingDate, userId, workIdResult, authorId));
            }

            return ratings;
        }

        public async Task<List<Rating>> GetAllByAuthorId(string authorId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT ratingId, value, ratingDate, userId, workId, authorId 
                  FROM ratings 
                  WHERE authorId = @authorId
                  ORDER BY ratingDate DESC", conn);

            cmd.Parameters.AddWithValue("authorId", authorId);

            using var reader = await cmd.ExecuteReaderAsync();

            var ratings = new List<Rating>();

            while (await reader.ReadAsync())
            {
                var ratingId = reader.GetString(0);
                var value = reader.GetDecimal(1);
                var ratingDate = reader.GetDateTime(2);
                var userId = reader.GetString(3);
                var workId = await reader.IsDBNullAsync(4) ? null : reader.GetString(4);
                var authorIdResult = await reader.IsDBNullAsync(5) ? null : reader.GetString(5);

                ratings.Add(new Rating(ratingId, value, ratingDate, userId, workId, authorIdResult));
            }

            return ratings;
        }

        public async Task CreateOne(Rating rating)
        {
            using var conn = _db.GetConnection();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                string sql = @"INSERT INTO ratings(ratingId, value, ratingDate, userId, workId, authorId) 
                               VALUES(@ratingId, @value, @ratingDate, @userId, @workId, @authorId)";

                using var cmd = new NpgsqlCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("ratingId", rating.RatingId);
                cmd.Parameters.AddWithValue("value", rating.Value);
                cmd.Parameters.AddWithValue("ratingDate", rating.RatingDate);
                cmd.Parameters.AddWithValue("userId", rating.UserId);
                cmd.Parameters.AddWithValue("workId", (object?)rating.WorkId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("authorId", (object?)rating.AuthorId ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateOne(Rating rating)
        {
            using var conn = _db.GetConnection();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                string sql = @"UPDATE ratings 
                               SET value = @value, 
                                   ratingDate = @ratingDate, 
                                   userId = @userId, 
                                   workId = @workId, 
                                   authorId = @authorId 
                               WHERE ratingId = @ratingId";

                using var cmd = new NpgsqlCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("ratingId", rating.RatingId);
                cmd.Parameters.AddWithValue("value", rating.Value);
                cmd.Parameters.AddWithValue("ratingDate", rating.RatingDate);
                cmd.Parameters.AddWithValue("userId", rating.UserId);
                cmd.Parameters.AddWithValue("workId", (object?)rating.WorkId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("authorId", (object?)rating.AuthorId ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteById(string ratingId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(@"DELETE FROM ratings WHERE ratingId = @ratingId", conn);
            cmd.Parameters.AddWithValue("ratingId", ratingId);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}