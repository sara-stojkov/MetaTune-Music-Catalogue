using Core.Model;
using Core.Storage;
using Npgsql;
using Task = System.Threading.Tasks.Task;

namespace PostgreSQLStorage
{
    public class ReviewStorage : IReviewStorage
    {
        private readonly Database _db;

        public ReviewStorage(Database db)
        {
            _db = db;
        }

        public async Task<Review?> GetById(string id)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT reviewId, content, reviewDate, isEditable, editorId, userId, workId, authorId 
                  FROM reviews 
                  WHERE reviewId = @id", conn);

            cmd.Parameters.AddWithValue("id", id);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var reviewId = reader.GetString(0);
                var content = reader.GetString(1);
                var reviewDate = reader.GetDateTime(2);
                var isEditable = reader.GetBoolean(3);
                var editorId = await reader.IsDBNullAsync(4) ? null : reader.GetString(4);
                var userId = reader.GetString(5);
                var workId = await reader.IsDBNullAsync(6) ? null : reader.GetString(6);
                var authorId = await reader.IsDBNullAsync(7) ? null : reader.GetString(7);

                return new Review(reviewId, content, reviewDate, isEditable, editorId, userId, workId, authorId);
            }

            return null;
        }

        public async Task<List<Review>> GetAll()
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT reviewId, content, reviewDate, isEditable, editorId, userId, workId, authorId 
                  FROM reviews 
                  ORDER BY reviewDate DESC", conn);

            using var reader = await cmd.ExecuteReaderAsync();

            var reviews = new List<Review>();

            while (await reader.ReadAsync())
            {
                var reviewId = reader.GetString(0);
                var content = reader.GetString(1);
                var reviewDate = reader.GetDateTime(2);
                var isEditable = reader.GetBoolean(3);
                var editorId = await reader.IsDBNullAsync(4) ? null : reader.GetString(4);
                var userId = reader.GetString(5);
                var workId = await reader.IsDBNullAsync(6) ? null : reader.GetString(6);
                var authorId = await reader.IsDBNullAsync(7) ? null : reader.GetString(7);

                reviews.Add(new Review(reviewId, content, reviewDate, isEditable, editorId, userId, workId, authorId));
            }

            return reviews;
        }

        public async Task<List<Review>> GetAllByUserId(string userId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT reviewId, content, reviewDate, isEditable, editorId, userId, workId, authorId 
                  FROM reviews 
                  WHERE userId = @userId
                  ORDER BY reviewDate DESC", conn);

            cmd.Parameters.AddWithValue("userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();

            var reviews = new List<Review>();

            while (await reader.ReadAsync())
            {
                var reviewId = reader.GetString(0);
                var content = reader.GetString(1);
                var reviewDate = reader.GetDateTime(2);
                var isEditable = reader.GetBoolean(3);
                var editorId = await reader.IsDBNullAsync(4) ? null : reader.GetString(4);
                var userIdResult = reader.GetString(5);
                var workId = await reader.IsDBNullAsync(6) ? null : reader.GetString(6);
                var authorId = await reader.IsDBNullAsync(7) ? null : reader.GetString(7);

                reviews.Add(new Review(reviewId, content, reviewDate, isEditable, editorId, userIdResult, workId, authorId));
            }

            return reviews;
        }

        public async Task<List<Review>> GetAllByWorkId(string workId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT reviewId, content, reviewDate, isEditable, editorId, userId, workId, authorId 
                  FROM reviews 
                  WHERE workId = @workId
                  ORDER BY reviewDate DESC", conn);

            cmd.Parameters.AddWithValue("workId", workId);

            using var reader = await cmd.ExecuteReaderAsync();

            var reviews = new List<Review>();

            while (await reader.ReadAsync())
            {
                var reviewId = reader.GetString(0);
                var content = reader.GetString(1);
                var reviewDate = reader.GetDateTime(2);
                var isEditable = reader.GetBoolean(3);
                var editorId = await reader.IsDBNullAsync(4) ? null : reader.GetString(4);
                var userId = reader.GetString(5);
                var workIdResult = await reader.IsDBNullAsync(6) ? null : reader.GetString(6);
                var authorId = await reader.IsDBNullAsync(7) ? null : reader.GetString(7);

                reviews.Add(new Review(reviewId, content, reviewDate, isEditable, editorId, userId, workIdResult, authorId));
            }

            return reviews;
        }

        public async Task<List<Review>> GetAllByAuthorId(string authorId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT reviewId, content, reviewDate, isEditable, editorId, userId, workId, authorId 
                  FROM reviews 
                  WHERE authorId = @authorId
                  ORDER BY reviewDate DESC", conn);

            cmd.Parameters.AddWithValue("authorId", authorId);

            using var reader = await cmd.ExecuteReaderAsync();

            var reviews = new List<Review>();

            while (await reader.ReadAsync())
            {
                var reviewId = reader.GetString(0);
                var content = reader.GetString(1);
                var reviewDate = reader.GetDateTime(2);
                var isEditable = reader.GetBoolean(3);
                var editorId = await reader.IsDBNullAsync(4) ? null : reader.GetString(4);
                var userId = reader.GetString(5);
                var workId = await reader.IsDBNullAsync(6) ? null : reader.GetString(6);
                var authorIdResult = await reader.IsDBNullAsync(7) ? null : reader.GetString(7);

                reviews.Add(new Review(reviewId, content, reviewDate, isEditable, editorId, userId, workId, authorIdResult));
            }

            return reviews;
        }

        public async Task<List<Review>> GetAllPending()
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT reviewId, content, reviewDate, isEditable, editorId, userId, workId, authorId 
                  FROM reviews 
                  WHERE editorId IS NULL
                  ORDER BY reviewDate DESC", conn);

            using var reader = await cmd.ExecuteReaderAsync();

            var reviews = new List<Review>();

            while (await reader.ReadAsync())
            {
                var reviewId = reader.GetString(0);
                var content = reader.GetString(1);
                var reviewDate = reader.GetDateTime(2);
                var isEditable = reader.GetBoolean(3);
                var editorId = await reader.IsDBNullAsync(4) ? null : reader.GetString(4);
                var userId = reader.GetString(5);
                var workId = await reader.IsDBNullAsync(6) ? null : reader.GetString(6);
                var authorId = await reader.IsDBNullAsync(7) ? null : reader.GetString(7);

                reviews.Add(new Review(reviewId, content, reviewDate, isEditable, editorId, userId, workId, authorId));
            }

            return reviews;
        }

        public async Task<Review?> GetEditorReviewByWorkId(string workId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT reviewId, content, reviewDate, isEditable, editorId, userId, workId, authorId 
                  FROM reviews 
                  WHERE workId = @workId AND editorId IS NOT NULL 
                  ORDER BY reviewDate DESC 
                  LIMIT 1", conn);

            cmd.Parameters.AddWithValue("workId", workId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var reviewId = reader.GetString(0);
                var content = reader.GetString(1);
                var reviewDate = reader.GetDateTime(2);
                var isEditable = reader.GetBoolean(3);
                var editorId = await reader.IsDBNullAsync(4) ? null : reader.GetString(4);
                var userId = reader.GetString(5);
                var workIdResult = await reader.IsDBNullAsync(6) ? null : reader.GetString(6);
                var authorId = await reader.IsDBNullAsync(7) ? null : reader.GetString(7);

                return new Review(reviewId, content, reviewDate, isEditable, editorId, userId, workIdResult, authorId);
            }

            return null;
        }

        public async Task CreateOne(Review review)
        {
            using var conn = _db.GetConnection();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                string sql = @"INSERT INTO reviews(reviewId, content, reviewDate, isEditable, editorId, userId, workId, authorId) 
                               VALUES(@reviewId, @content, @reviewDate, @isEditable, @editorId, @userId, @workId, @authorId)";

                using var cmd = new NpgsqlCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("reviewId", review.ReviewId);
                cmd.Parameters.AddWithValue("content", review.Content);
                cmd.Parameters.AddWithValue("reviewDate", review.ReviewDate);
                cmd.Parameters.AddWithValue("isEditable", review.IsEditable);
                cmd.Parameters.AddWithValue("editorId", (object?)review.UserId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("userId", review.UserId2);
                cmd.Parameters.AddWithValue("workId", (object?)review.WorkId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("authorId", (object?)review.AuthorId ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateOne(Review review)
        {
            using var conn = _db.GetConnection();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                string sql = @"UPDATE reviews 
                               SET content = @content, 
                                   reviewDate = @reviewDate, 
                                   isEditable = @isEditable, 
                                   editorId = @editorId, 
                                   userId = @userId, 
                                   workId = @workId, 
                                   authorId = @authorId 
                               WHERE reviewId = @reviewId";

                using var cmd = new NpgsqlCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("reviewId", review.ReviewId);
                cmd.Parameters.AddWithValue("content", review.Content);
                cmd.Parameters.AddWithValue("reviewDate", review.ReviewDate);
                cmd.Parameters.AddWithValue("isEditable", review.IsEditable);
                cmd.Parameters.AddWithValue("editorId", (object?)review.UserId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("userId", review.UserId2);
                cmd.Parameters.AddWithValue("workId", (object?)review.WorkId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("authorId", (object?)review.AuthorId ?? DBNull.Value);

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
            using var cmd = new NpgsqlCommand(@"DELETE FROM reviews WHERE reviewId = @id", conn);
            cmd.Parameters.AddWithValue("id", id);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}