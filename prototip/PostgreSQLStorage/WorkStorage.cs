using Core.Model;
using Core.Storage;
using Npgsql;

namespace PostgreSQLStorage
{
    public class WorkStorage(Database database) : IWorkStorage
    {
        private readonly Database _db = database;

        public async Task<Work?> GetById(string workId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                "SELECT * FROM works WHERE workId = @id", conn);
            cmd.Parameters.AddWithValue("id", workId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Work(
                    reader.GetString(reader.GetOrdinal("workId")),
                    reader.GetString(reader.GetOrdinal("workName")),
                    reader.GetDateTime(reader.GetOrdinal("publishDate")),
                    reader.GetString(reader.GetOrdinal("workType")),
                    reader.GetString(reader.GetOrdinal("genreId")),
                    new List<Author>(), // Will be populated separately
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
            }
            return null;
        }

        public async Task<List<Work>> GetAll()
        {
            var works = new List<Work>();
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                "SELECT * FROM works ORDER BY publishDate DESC", conn);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                works.Add(new Work(
                    reader.GetString(reader.GetOrdinal("workId")),
                    reader.GetString(reader.GetOrdinal("workName")),
                    reader.GetDateTime(reader.GetOrdinal("publishDate")),
                    reader.GetString(reader.GetOrdinal("workType")),
                    reader.GetString(reader.GetOrdinal("genreId")),
                    new List<Author>(),
                    await reader.IsDBNullAsync(reader.GetOrdinal("workDescription"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("workDescription")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("src"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("src")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("albumId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("albumId"))
                ));
            }
            return works;
        }

        public async Task<List<Work>> GetByAlbumId(string albumId)
        {
            var works = new List<Work>();
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                "SELECT * FROM works WHERE albumId = @albumId ORDER BY publishDate", conn);
            cmd.Parameters.AddWithValue("albumId", albumId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                works.Add(new Work(
                    reader.GetString(reader.GetOrdinal("workId")),
                    reader.GetString(reader.GetOrdinal("workName")),
                    reader.GetDateTime(reader.GetOrdinal("publishDate")),
                    reader.GetString(reader.GetOrdinal("workType")),
                    reader.GetString(reader.GetOrdinal("genreId")),
                    new List<Author>(),
                    await reader.IsDBNullAsync(reader.GetOrdinal("workDescription"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("workDescription")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("src"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("src")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("albumId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("albumId"))
                ));
            }
            return works;
        }

        public async Task<List<Author>> GetWorkAuthors(string workId)
        {
            var authors = new List<Author>();
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT a.* FROM authors a
                  INNER JOIN performs p ON a.authorId = p.authorId
                  WHERE p.workId = @workId", conn);
            cmd.Parameters.AddWithValue("workId", workId);

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

        public async Task<List<Contributor>> GetWorkContributors(string workId)
        {
            var contributors = new List<Contributor>();
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT c.*, p.personName, p.personSurname 
                  FROM contributors c
                  INNER JOIN people p ON c.personId = p.personId
                  WHERE c.workId = @workId", conn);
            cmd.Parameters.AddWithValue("workId", workId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                contributors.Add(new Contributor(
                    reader.GetString(reader.GetOrdinal("contributionType")),
                    reader.GetString(reader.GetOrdinal("personId")),
                    reader.GetString(reader.GetOrdinal("workId"))
                ));
            }
            return contributors;
        }

        public async Task<decimal?> GetAverageRating(string workId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                "SELECT AVG(value) FROM ratings WHERE workId = @workId", conn);
            cmd.Parameters.AddWithValue("workId", workId);

            var result = await cmd.ExecuteScalarAsync();
            return result == DBNull.Value ? null : Convert.ToDecimal(result);
        }

        public async Task<Rating?> GetUserRating(string workId, string userId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                "SELECT * FROM ratings WHERE workId = @workId AND userId = @userId", conn);
            cmd.Parameters.AddWithValue("workId", workId);
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

        public async Task<Review?> GetEditorReview(string workId)
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

        public async Task<List<Review>> GetUserReviews(string workId)
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

        public async Task<Genre?> GetWorkGenre(string workId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT g.* FROM genres g
                  INNER JOIN works w ON g.genreId = w.genreId
                  WHERE w.workId = @workId", conn);
            cmd.Parameters.AddWithValue("workId", workId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Genre(
                    reader.GetString(reader.GetOrdinal("genreId")),
                    reader.GetString(reader.GetOrdinal("genreName")),
                    await reader.IsDBNullAsync(reader.GetOrdinal("genreDescription"))
                        ? ""
                        : reader.GetString(reader.GetOrdinal("genreDescription"))
                );
            }
            return null;
        }

        public async System.Threading.Tasks.Task Add(Work work)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"INSERT INTO works (workId, workName, publishDate, workType, genreId, workDescription, src, albumId)
                  VALUES (@id, @name, @date, @type, @genreId, @desc, @src, @albumId)", conn);
            cmd.Parameters.AddWithValue("id", work.WorkId);
            cmd.Parameters.AddWithValue("name", work.WorkName);
            cmd.Parameters.AddWithValue("date", work.PublishDate);
            cmd.Parameters.AddWithValue("type", work.WorkType);
            cmd.Parameters.AddWithValue("genreId", work.GenreId);
            cmd.Parameters.AddWithValue("desc", (object?)work.WorkDescription ?? DBNull.Value);
            cmd.Parameters.AddWithValue("src", (object?)work.Src ?? DBNull.Value);
            cmd.Parameters.AddWithValue("albumId", (object?)work.AlbumId ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }

        public async System.Threading.Tasks.Task Update(Work work)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"UPDATE works 
                  SET workName = @name, publishDate = @date, workType = @type, 
                      genreId = @genreId, workDescription = @desc, src = @src, albumId = @albumId
                  WHERE workId = @id", conn);
            cmd.Parameters.AddWithValue("id", work.WorkId);
            cmd.Parameters.AddWithValue("name", work.WorkName);
            cmd.Parameters.AddWithValue("date", work.PublishDate);
            cmd.Parameters.AddWithValue("type", work.WorkType);
            cmd.Parameters.AddWithValue("genreId", work.GenreId);
            cmd.Parameters.AddWithValue("desc", (object?)work.WorkDescription ?? DBNull.Value);
            cmd.Parameters.AddWithValue("src", (object?)work.Src ?? DBNull.Value);
            cmd.Parameters.AddWithValue("albumId", (object?)work.AlbumId ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
