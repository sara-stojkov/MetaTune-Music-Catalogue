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

        public async Task<Review?> GetById(string reviewId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT reviewId, content, reviewDate, isEditable, userId, userId2, workId, authorId 
                  FROM reviews 
                  WHERE reviewId = @reviewId", conn);

            cmd.Parameters.AddWithValue("reviewId", reviewId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapReaderToReview(reader);
            }

            return null;
        }

        public async Task<List<Review>> GetAll()
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT reviewId, content, reviewDate, isEditable, userId, userId2, workId, authorId 
                  FROM reviews 
                  ORDER BY reviewDate DESC", conn);

            using var reader = await cmd.ExecuteReaderAsync();

            var reviews = new List<Review>();

            while (await reader.ReadAsync())
            {
                reviews.Add(MapReaderToReview(reader));
            }

            return reviews;
        }

        public async Task CreateOne(Review review)
        {
            using var conn = _db.GetConnection();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                string sql = @"INSERT INTO reviews(reviewId, content, reviewDate, isEditable, userId, userId2, workId, authorId) 
                               VALUES(@reviewId, @content, @reviewDate, @isEditable, @userId, @userId2, @workId, @authorId)";

                using var cmd = new NpgsqlCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("reviewId", review.ReviewId);
                cmd.Parameters.AddWithValue("content", review.Content);
                cmd.Parameters.AddWithValue("reviewDate", review.ReviewDate);
                cmd.Parameters.AddWithValue("isEditable", review.IsEditable);
                cmd.Parameters.AddWithValue("userId", (object?)review.UserId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("userId2", review.UserId2);
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
                                   userId = @userId, 
                                   userId2 = @userId2, 
                                   workId = @workId, 
                                   authorId = @authorId 
                               WHERE reviewId = @reviewId";

                using var cmd = new NpgsqlCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("reviewId", review.ReviewId);
                cmd.Parameters.AddWithValue("content", review.Content);
                cmd.Parameters.AddWithValue("reviewDate", review.ReviewDate);
                cmd.Parameters.AddWithValue("isEditable", review.IsEditable);
                cmd.Parameters.AddWithValue("userId", (object?)review.UserId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("userId2", review.UserId2);
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

        public async Task DeleteById(string reviewId)
        {
            using var conn = _db.GetConnection();
            string sql = @"DELETE FROM reviews WHERE reviewId = @reviewId";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("reviewId", reviewId);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<Review>> GetAllByUserId(string userId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT reviewId, content, reviewDate, isEditable, userId, userId2, workId, authorId 
                  FROM reviews 
                  WHERE userId2 = @userId 
                  ORDER BY reviewDate DESC", conn);

            cmd.Parameters.AddWithValue("userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();

            var reviews = new List<Review>();

            while (await reader.ReadAsync())
            {
                reviews.Add(MapReaderToReview(reader));
            }

            return reviews;
        }

        public async Task<List<Review>> GetAllByWorkId(string workId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT reviewId, content, reviewDate, isEditable, userId, userId2, workId, authorId 
                  FROM reviews 
                  WHERE workId = @workId 
                  ORDER BY reviewDate DESC", conn);

            cmd.Parameters.AddWithValue("workId", workId);

            using var reader = await cmd.ExecuteReaderAsync();

            var reviews = new List<Review>();

            while (await reader.ReadAsync())
            {
                reviews.Add(MapReaderToReview(reader));
            }

            return reviews;
        }

        public async Task<List<Review>> GetAllByAuthorId(string artistId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT reviewId, content, reviewDate, isEditable, userId, userId2, workId, authorId 
                  FROM reviews 
                  WHERE authorId = @artistId 
                  ORDER BY reviewDate DESC", conn);

            cmd.Parameters.AddWithValue("artistId", artistId);

            using var reader = await cmd.ExecuteReaderAsync();

            var reviews = new List<Review>();

            while (await reader.ReadAsync())
            {
                reviews.Add(MapReaderToReview(reader));
            }

            return reviews;
        }

        public async Task<Review?> GetEditorReviewByWorkId(string workId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT reviewId, content, reviewDate, isEditable, userId, userId2, workId, authorId 
                  FROM reviews 
                  WHERE workId = @workId AND userId IS NOT NULL 
                  ORDER BY reviewDate DESC 
                  LIMIT 1", conn);

            cmd.Parameters.AddWithValue("workId", workId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapReaderToReview(reader);
            }

            return null;
        }

        private Review MapReaderToReview(NpgsqlDataReader reader)
        {
            var reviewId = reader.GetString(0);
            var content = reader.GetString(1);
            var reviewDate = reader.GetDateTime(2);
            var isEditable = reader.GetBoolean(3);
            var userId = reader.IsDBNull(4) ? null : reader.GetString(4);
            var userId2 = reader.GetString(5);
            var workId = reader.IsDBNull(6) ? null : reader.GetString(6);
            var authorId = reader.IsDBNull(7) ? null : reader.GetString(7);

            return new Review(reviewId, content, reviewDate, isEditable, userId, userId2, workId, authorId);
        }
    }
}