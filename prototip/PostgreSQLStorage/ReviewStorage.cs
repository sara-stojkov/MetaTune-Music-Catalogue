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
                @"SELECT r.reviewid, r.content, r.reviewdate, r.iseditable, r.editorid, r.""userId"", r.workid, r.authorid,
                         CASE 
                             WHEN COALESCE(u.contactvisible, true) = true THEN CONCAT(p.personname, ' ', p.personsurname)
                             ELSE 'Onaj ko ne sme biti imenovan'
                         END as displayname
                  FROM reviews r
                  INNER JOIN users u ON r.""userId"" = u.userid
                  INNER JOIN people p ON u.personid = p.personid
                  WHERE r.reviewid = @id AND COALESCE(u.reviewsvisible, true) = true", conn);

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
                var displayName = reader.GetString(8);

                return new Review(reviewId, content, reviewDate, isEditable, editorId, userId, workId, authorId, displayName);
            }

            return null;
        }

        public async Task<List<Review>> GetAll()
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT r.reviewid, r.content, r.reviewdate, r.iseditable, r.editorid, r.""userId"", r.workid, r.authorid,
                         CASE 
                             WHEN COALESCE(u.contactvisible, true) = true THEN CONCAT(p.personname, ' ', p.personsurname)
                             ELSE 'Onaj ko ne sme biti imenovan'
                         END as displayname
                  FROM reviews r
                  INNER JOIN users u ON r.""userId"" = u.userid
                  INNER JOIN people p ON u.personid = p.personid
                  WHERE COALESCE(u.reviewsvisible, true) = true
                  ORDER BY r.reviewdate DESC", conn);

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
                var displayName = reader.GetString(8);

                reviews.Add(new Review(reviewId, content, reviewDate, isEditable, editorId, userId, workId, authorId, displayName));
            }

            return reviews;
        }

        public async Task<List<Review>> GetAllByUserId(string userId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT r.reviewid, r.content, r.reviewdate, r.iseditable, r.editorid, r.""userId"", r.workid, r.authorid,
                         CASE 
                             WHEN COALESCE(u.contactvisible, true) = true THEN CONCAT(p.personname, ' ', p.personsurname)
                             ELSE 'Onaj ko ne sme biti imenovan'
                         END as displayname
                  FROM reviews r
                  INNER JOIN users u ON r.""userId"" = u.userid
                  INNER JOIN people p ON u.personid = p.personid
                  WHERE r.""userId"" = @userId AND COALESCE(u.reviewsvisible, true) = true
                  ORDER BY r.reviewdate DESC", conn);

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
                var displayName = reader.GetString(8);

                reviews.Add(new Review(reviewId, content, reviewDate, isEditable, editorId, userIdResult, workId, authorId, displayName));
            }

            return reviews;
        }

        public async Task<List<Review>> GetAllByWorkId(string workId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT r.reviewid, r.content, r.reviewdate, r.iseditable, r.editorid, r.""userId"", r.workid, r.authorid,
                         CASE 
                             WHEN COALESCE(u.contactvisible, true) = true THEN CONCAT(p.personname, ' ', p.personsurname)
                             ELSE 'Onaj ko ne sme biti imenovan'
                         END as displayname
                  FROM reviews r
                  INNER JOIN users u ON r.""userId"" = u.userid
                  INNER JOIN people p ON u.personid = p.personid
                  WHERE r.workid = @workId AND COALESCE(u.reviewsvisible, true) = true
                  ORDER BY r.reviewdate DESC", conn);

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
                var displayName = reader.GetString(8);

                reviews.Add(new Review(reviewId, content, reviewDate, isEditable, editorId, userId, workIdResult, authorId, displayName));
            }

            return reviews;
        }

        public async Task<List<Review>> GetAllApprovedByWorkId(string workId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT r.reviewid, r.content, r.reviewdate, r.iseditable, r.editorid, r.""userId"", r.workid, r.authorid,
                         CASE 
                             WHEN COALESCE(u.contactvisible, true) = true THEN CONCAT(p.personname, ' ', p.personsurname)
                             ELSE 'Onaj ko ne sme biti imenovan'
                         END as displayname
                  FROM reviews r
                  INNER JOIN users u ON r.""userId"" = u.userid
                  INNER JOIN people p ON u.personid = p.personid
                  WHERE r.workid = @workId AND r.editorid IS NOT NULL AND COALESCE(u.reviewsvisible, true) = true
                  ORDER BY r.reviewdate DESC", conn);

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
                var displayName = reader.GetString(8);

                reviews.Add(new Review(reviewId, content, reviewDate, isEditable, editorId, userId, workIdResult, authorId, displayName));
            }

            return reviews;
        }

        public async Task<List<Review>> GetAllByAuthorId(string authorId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT r.reviewid, r.content, r.reviewdate, r.iseditable, r.editorid, r.""userId"", r.workid, r.authorid,
                         CASE 
                             WHEN COALESCE(u.contactvisible, true) = true THEN CONCAT(p.personname, ' ', p.personsurname)
                             ELSE 'Onaj ko ne sme biti imenovan'
                         END as displayname
                  FROM reviews r
                  INNER JOIN users u ON r.""userId"" = u.userid
                  INNER JOIN people p ON u.personid = p.personid
                  WHERE r.authorid = @authorId AND COALESCE(u.reviewsvisible, true) = true
                  ORDER BY r.reviewdate DESC", conn);

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
                var displayName = reader.GetString(8);

                reviews.Add(new Review(reviewId, content, reviewDate, isEditable, editorId, userId, workId, authorIdResult, displayName));
            }

            return reviews;
        }

        public async Task<List<Review>> GetAllApprovedByAuthorId(string authorId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT r.reviewid, r.content, r.reviewdate, r.iseditable, r.editorid, r.""userId"", r.workid, r.authorid,
                         CASE 
                             WHEN COALESCE(u.contactvisible, true) = true THEN CONCAT(p.personname, ' ', p.personsurname)
                             ELSE 'Onaj ko ne sme biti imenovan'
                         END as displayname
                  FROM reviews r
                  INNER JOIN users u ON r.""userId"" = u.userid
                  INNER JOIN people p ON u.personid = p.personid
                  WHERE r.authorid = @authorId AND r.editorid IS NOT NULL AND COALESCE(u.reviewsvisible, true) = true
                  ORDER BY r.reviewdate DESC", conn);

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
                var displayName = reader.GetString(8);

                reviews.Add(new Review(reviewId, content, reviewDate, isEditable, editorId, userId, workId, authorIdResult, displayName));
            }

            return reviews;
        }

        public async Task<List<Review>> GetAllPending()
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT r.reviewid, r.content, r.reviewdate, r.iseditable, r.editorid, r.""userId"", r.workid, r.authorid,
                         CASE 
                             WHEN COALESCE(u.contactvisible, true) = true THEN CONCAT(p.personname, ' ', p.personsurname)
                             ELSE 'Onaj ko ne sme biti imenovan'
                         END as displayname
                  FROM reviews r
                  INNER JOIN users u ON r.""userId"" = u.userid
                  INNER JOIN people p ON u.personid = p.personid
                  WHERE r.editorid IS NULL AND COALESCE(u.reviewsvisible, true) = true
                  ORDER BY r.reviewdate DESC", conn);

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
                var displayName = reader.GetString(8);

                reviews.Add(new Review(reviewId, content, reviewDate, isEditable, editorId, userId, workId, authorId, displayName));
            }

            return reviews;
        }

        public async Task<Review?> GetEditorReviewByWorkId(string workId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT r.reviewid, r.content, r.reviewdate, r.iseditable, r.editorid, r.""userId"", r.workid, r.authorid,
                         CASE 
                             WHEN COALESCE(u.contactvisible, true) = true THEN CONCAT(p.personname, ' ', p.personsurname)
                             ELSE 'Onaj ko ne sme biti imenovan'
                         END as displayname
                  FROM reviews r
                  INNER JOIN users u ON r.""userId"" = u.userid
                  INNER JOIN people p ON u.personid = p.personid
                  WHERE r.workid = @workId 
                    AND r.editorid = r.""userId"" 
                    AND COALESCE(u.reviewsvisible, true) = true
                  ORDER BY r.reviewdate DESC 
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
                var displayName = reader.GetString(8);

                return new Review(reviewId, content, reviewDate, isEditable, editorId, userId, workIdResult, authorId, displayName);
            }

            return null;
        }

        public async Task<Review?> GetEditorReviewByAuthorId(string authorId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT r.reviewid, r.content, r.reviewdate, r.iseditable, r.editorid, r.""userId"", r.workid, r.authorid,
                         CASE 
                             WHEN COALESCE(u.contactvisible, true) = true THEN CONCAT(p.personname, ' ', p.personsurname)
                             ELSE 'Onaj ko ne sme biti imenovan'
                         END as displayname
                  FROM reviews r
                  INNER JOIN users u ON r.""userId"" = u.userid
                  INNER JOIN people p ON u.personid = p.personid
                  WHERE r.authorid = @authorId 
                    AND r.editorid = r.""userId"" 
                    AND COALESCE(u.reviewsvisible, true) = true
                  ORDER BY r.reviewdate DESC 
                  LIMIT 1", conn);

            cmd.Parameters.AddWithValue("authorId", authorId);

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
                var authorIdResult = await reader.IsDBNullAsync(7) ? null : reader.GetString(7);
                var displayName = reader.GetString(8);

                return new Review(reviewId, content, reviewDate, isEditable, editorId, userId, workId, authorIdResult, displayName);
            }

            return null;
        }

        public async Task CreateOne(Review review)
        {
            using var conn = _db.GetConnection();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                string sql = @"INSERT INTO reviews(reviewid, content, reviewdate, iseditable, editorid, ""userId"", workid, authorid) 
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
                                   reviewdate = @reviewDate, 
                                   iseditable = @isEditable, 
                                   editorid = @editorId
                               WHERE reviewid = @reviewId";

                using var cmd = new NpgsqlCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("reviewId", review.ReviewId);
                cmd.Parameters.AddWithValue("content", review.Content);
                cmd.Parameters.AddWithValue("reviewDate", review.ReviewDate);
                cmd.Parameters.AddWithValue("isEditable", review.IsEditable);
                cmd.Parameters.AddWithValue("editorId", (object?)review.UserId ?? DBNull.Value);

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
            using var cmd = new NpgsqlCommand(@"DELETE FROM reviews WHERE reviewid = @id", conn);
            cmd.Parameters.AddWithValue("id", id);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}