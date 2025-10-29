using Core.Model;
using Core.Storage;
using Npgsql;

namespace PostgreSQLStorage
{
    public class ReviewStorage(Database database) : IReviewStorage
    {
        private readonly Database _db = database;

        public async Task<Review?> GetById(string reviewId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                "SELECT * FROM reviews WHERE reviewId = @id", conn);
            cmd.Parameters.AddWithValue("id", reviewId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Review(
                    reader.GetString(reader.GetOrdinal("reviewId")),
                    reader.GetString(reader.GetOrdinal("content")),
                    reader.GetDateTime(reader.GetOrdinal("reviewDate")),
                    reader.GetBoolean(reader.GetOrdinal("isEditable")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("userId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("userId")),
                    reader.GetString(reader.GetOrdinal("userId2")),
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

        public async Task<List<Review>> GetByAuthorId(string authorId)
        {
            var reviews = new List<Review>();
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                "SELECT * FROM reviews WHERE authorId = @authorId AND isEditable = true ORDER BY reviewDate DESC", conn);
            cmd.Parameters.AddWithValue("authorId", authorId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                reviews.Add(new Review(
                    reader.GetString(reader.GetOrdinal("reviewId")),
                    reader.GetString(reader.GetOrdinal("content")),
                    reader.GetDateTime(reader.GetOrdinal("reviewDate")),
                    reader.GetBoolean(reader.GetOrdinal("isEditable")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("userId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("userId")),
                    reader.GetString(reader.GetOrdinal("userId2")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("workId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("workId")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("authorId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("authorId"))
                ));
            }
            return reviews;
        }

        public async Task<List<Review>> GetByWorkId(string workId)
        {
            var reviews = new List<Review>();
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                "SELECT * FROM reviews WHERE workId = @workId AND isEditable = true ORDER BY reviewDate DESC", conn);
            cmd.Parameters.AddWithValue("workId", workId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                reviews.Add(new Review(
                    reader.GetString(reader.GetOrdinal("reviewId")),
                    reader.GetString(reader.GetOrdinal("content")),
                    reader.GetDateTime(reader.GetOrdinal("reviewDate")),
                    reader.GetBoolean(reader.GetOrdinal("isEditable")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("userId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("userId")),
                    reader.GetString(reader.GetOrdinal("userId2")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("workId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("workId")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("authorId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("authorId"))
                ));
            }
            return reviews;
        }

        public async Task<Review?> GetEditorReviewForAuthor(string authorId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT r.* FROM reviews r
                  INNER JOIN users u ON r.userId2 = u.userId
                  WHERE r.authorId = @authorId AND u.role = @editorRole
                  AND r.isEditable = true
                  ORDER BY r.reviewDate DESC
                  LIMIT 1", conn);
            cmd.Parameters.AddWithValue("authorId", authorId);
            cmd.Parameters.AddWithValue("editorRole", UserRole.EDITOR);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Review(
                    reader.GetString(reader.GetOrdinal("reviewId")),
                    reader.GetString(reader.GetOrdinal("content")),
                    reader.GetDateTime(reader.GetOrdinal("reviewDate")),
                    reader.GetBoolean(reader.GetOrdinal("isEditable")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("userId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("userId")),
                    reader.GetString(reader.GetOrdinal("userId2")),
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

        public async Task<Review?> GetEditorReviewForWork(string workId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT r.* FROM reviews r
                  INNER JOIN users u ON r.userId2 = u.userId
                  WHERE r.workId = @workId AND u.role = @editorRole
                  AND r.isEditable = true
                  ORDER BY r.reviewDate DESC
                  LIMIT 1", conn);
            cmd.Parameters.AddWithValue("workId", workId);
            cmd.Parameters.AddWithValue("editorRole", UserRole.EDITOR);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Review(
                    reader.GetString(reader.GetOrdinal("reviewId")),
                    reader.GetString(reader.GetOrdinal("content")),
                    reader.GetDateTime(reader.GetOrdinal("reviewDate")),
                    reader.GetBoolean(reader.GetOrdinal("isEditable")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("userId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("userId")),
                    reader.GetString(reader.GetOrdinal("userId2")),
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

        public async Task<List<Review>> GetUserReviewsForAuthor(string authorId)
        {
            var reviews = new List<Review>();
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT r.* FROM reviews r
                  INNER JOIN users u ON r.userId2 = u.userId
                  WHERE r.authorId = @authorId AND u.role = @basicRole
                  AND r.isEditable = true
                  ORDER BY r.reviewDate DESC", conn);
            cmd.Parameters.AddWithValue("authorId", authorId);
            cmd.Parameters.AddWithValue("basicRole", UserRole.BASIC);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                reviews.Add(new Review(
                    reader.GetString(reader.GetOrdinal("reviewId")),
                    reader.GetString(reader.GetOrdinal("content")),
                    reader.GetDateTime(reader.GetOrdinal("reviewDate")),
                    reader.GetBoolean(reader.GetOrdinal("isEditable")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("userId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("userId")),
                    reader.GetString(reader.GetOrdinal("userId2")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("workId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("workId")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("authorId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("authorId"))
                ));
            }
            return reviews;
        }

        public async Task<List<Review>> GetUserReviewsForWork(string workId)
        {
            var reviews = new List<Review>();
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT r.* FROM reviews r
                  INNER JOIN users u ON r.userId2 = u.userId
                  WHERE r.workId = @workId AND u.role = @basicRole
                  AND r.isEditable = true
                  ORDER BY r.reviewDate DESC", conn);
            cmd.Parameters.AddWithValue("workId", workId);
            cmd.Parameters.AddWithValue("basicRole", UserRole.BASIC);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                reviews.Add(new Review(
                    reader.GetString(reader.GetOrdinal("reviewId")),
                    reader.GetString(reader.GetOrdinal("content")),
                    reader.GetDateTime(reader.GetOrdinal("reviewDate")),
                    reader.GetBoolean(reader.GetOrdinal("isEditable")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("userId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("userId")),
                    reader.GetString(reader.GetOrdinal("userId2")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("workId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("workId")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("authorId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("authorId"))
                ));
            }
            return reviews;
        }

        public async Task<Review?> GetUserReview(string userId, string? authorId, string? workId)
        {
            using var conn = _db.GetConnection();

            string query = "SELECT * FROM reviews WHERE userId2 = @userId";
            if (authorId != null)
                query += " AND authorId = @authorId";
            if (workId != null)
                query += " AND workId = @workId";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("userId", userId);
            if (authorId != null)
                cmd.Parameters.AddWithValue("authorId", authorId);
            if (workId != null)
                cmd.Parameters.AddWithValue("workId", workId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Review(
                    reader.GetString(reader.GetOrdinal("reviewId")),
                    reader.GetString(reader.GetOrdinal("content")),
                    reader.GetDateTime(reader.GetOrdinal("reviewDate")),
                    reader.GetBoolean(reader.GetOrdinal("isEditable")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("userId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("userId")),
                    reader.GetString(reader.GetOrdinal("userId2")),
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

        public async System.Threading.Tasks.Task Add(Review review)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"INSERT INTO reviews (reviewId, content, reviewDate, isEditable, userId, userId2, workId, authorId)
                  VALUES (@id, @content, @date, @editable, @userId1, @userId2, @workId, @authorId)", conn);
            cmd.Parameters.AddWithValue("id", review.ReviewId);
            cmd.Parameters.AddWithValue("content", review.Content);
            cmd.Parameters.AddWithValue("date", review.ReviewDate);
            cmd.Parameters.AddWithValue("editable", review.IsEditable);
            cmd.Parameters.AddWithValue("userId1", (object?)review.UserId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("userId2", review.UserId2);
            cmd.Parameters.AddWithValue("workId", (object?)review.WorkId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("authorId", (object?)review.AuthorId ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }

        public async System.Threading.Tasks.Task Update(Review review)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"UPDATE reviews 
                  SET content = @content, reviewDate = @date, isEditable = @editable
                  WHERE reviewId = @id", conn);
            cmd.Parameters.AddWithValue("id", review.ReviewId);
            cmd.Parameters.AddWithValue("content", review.Content);
            cmd.Parameters.AddWithValue("date", review.ReviewDate);
            cmd.Parameters.AddWithValue("editable", review.IsEditable);

            await cmd.ExecuteNonQueryAsync();
        }

        public async System.Threading.Tasks.Task Delete(string reviewId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                "DELETE FROM reviews WHERE reviewId = @id", conn);
            cmd.Parameters.AddWithValue("id", reviewId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async System.Threading.Tasks.Task Approve(string reviewId, string editorId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"UPDATE reviews 
                  SET isEditable = true, userId = @editorId
                  WHERE reviewId = @id", conn);
            cmd.Parameters.AddWithValue("id", reviewId);
            cmd.Parameters.AddWithValue("editorId", editorId);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
