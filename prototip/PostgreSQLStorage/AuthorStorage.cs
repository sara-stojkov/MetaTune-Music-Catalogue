using Core.Model;
using Core.Storage;
using Npgsql;

namespace PostgreSQLStorage
{
    public class AuthorStorage(Database database) : IAuthorStorage
    {
        private readonly Database _db = database;

        public async Task<Author?> GetById(string authorId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                "SELECT * FROM authors WHERE authorId = @id", conn);
            cmd.Parameters.AddWithValue("id", authorId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Author(
                    reader.GetString(reader.GetOrdinal("authorId")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("authorName"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("authorName")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("biography"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("biography")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("personId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("personId"))
                );
            }
            return null;
        }

        public async Task<List<Author>> GetAll()
        {
            var authors = new List<Author>();
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand("SELECT * FROM authors ORDER BY authorName", conn);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                authors.Add(new Author(
                    reader.GetString(reader.GetOrdinal("authorId")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("authorName"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("authorName")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("biography"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("biography")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("personId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("personId"))
                ));
            }
            return authors;
        }

        public async Task<List<Genre>> GetAuthorGenres(string authorId)
        {
            var genres = new List<Genre>();
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT g.genreId, g.genreName, g.genreDescription, g.parentGenreId 
                  FROM genres g 
                  INNER JOIN belongs b ON g.genreId = b.genreId 
                  WHERE b.authorId = @authorId", conn);
            cmd.Parameters.AddWithValue("authorId", authorId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                genres.Add(new Genre(
                    reader.GetString(reader.GetOrdinal("genreId")),
                    reader.GetString(reader.GetOrdinal("genreName")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("genreDescription"))
                        ? ""
                        : reader.GetString(reader.GetOrdinal("genreDescription"))
                ));
            }
            return genres;
        }

        public async Task<List<Work>> GetAuthorWorks(string authorId)
        {
            var works = new List<Work>();
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT w.* FROM works w
                  INNER JOIN performs p ON w.workId = p.workId
                  WHERE p.authorId = @authorId
                  ORDER BY w.publishDate DESC", conn);
            cmd.Parameters.AddWithValue("authorId", authorId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var work = new Work(
                    reader.GetString(reader.GetOrdinal("workId")),
                    reader.GetString(reader.GetOrdinal("workName")),
                    reader.GetDateTime(reader.GetOrdinal("publishDate")),
                    reader.GetString(reader.GetOrdinal("workType")),
                    reader.GetString(reader.GetOrdinal("genreId")),
                    new List<Author>(), // Will be populated separately if needed
                    await reader.IsDBNullAsync(reader.GetOrdinal("workDescription"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("workDescription")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("src"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("src")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("albumId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("albumId"))
                );
                works.Add(work);
            }
            return works;
        }

        public async Task<List<Member>> GetAuthorMemberships(string authorId)
        {
            var memberships = new List<Member>();
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT * FROM members 
                  WHERE memberId = @authorId OR groupId = @authorId
                  ORDER BY joinDate DESC", conn);
            cmd.Parameters.AddWithValue("authorId", authorId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                memberships.Add(new Member(
                    reader.GetDateTime(reader.GetOrdinal("joinDate")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("leaveDate"))
                        ? null
                        : reader.GetDateTime(reader.GetOrdinal("leaveDate")),
                    reader.GetString(reader.GetOrdinal("groupId")),
                    reader.GetString(reader.GetOrdinal("memberId"))
                ));
            }
            return memberships;
        }

        public async Task<decimal?> GetAverageRating(string authorId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                "SELECT AVG(value) FROM ratings WHERE authorId = @authorId", conn);
            cmd.Parameters.AddWithValue("authorId", authorId);

            var result = await cmd.ExecuteScalarAsync();
            return result == DBNull.Value ? null : Convert.ToDecimal(result);
        }

        public async Task<Rating?> GetUserRating(string authorId, string userId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                "SELECT * FROM ratings WHERE authorId = @authorId AND userId = @userId", conn);
            cmd.Parameters.AddWithValue("authorId", authorId);
            cmd.Parameters.AddWithValue("userId", userId);

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

        public async Task<Review?> GetEditorReview(string authorId)
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

        public async Task<List<Review>> GetUserReviews(string authorId)
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

        public async System.Threading.Tasks.Task Add(Author author)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"INSERT INTO authors (authorId, authorName, biography, personId)
                  VALUES (@id, @name, @bio, @personId)", conn);
            cmd.Parameters.AddWithValue("id", author.AuthorId);
            cmd.Parameters.AddWithValue("name", (object?)author.AuthorName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("bio", (object?)author.Biography ?? DBNull.Value);
            cmd.Parameters.AddWithValue("personId", (object?)author.PersonId ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }

        public async System.Threading.Tasks.Task Update(Author author)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"UPDATE authors 
                  SET authorName = @name, biography = @bio, personId = @personId
                  WHERE authorId = @id", conn);
            cmd.Parameters.AddWithValue("id", author.AuthorId);
            cmd.Parameters.AddWithValue("name", (object?)author.AuthorName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("bio", (object?)author.Biography ?? DBNull.Value);
            cmd.Parameters.AddWithValue("personId", (object?)author.PersonId ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
