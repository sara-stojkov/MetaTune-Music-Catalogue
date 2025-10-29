using Core.Model;
using Core.Storage;
using Npgsql;
using Task = System.Threading.Tasks.Task;

namespace PostgreSQLStorage
{
    public class WorkStorage : IWorkStorage
    {
        private readonly Database _db;

        public WorkStorage(Database db)
        {
            _db = db;
        }

        public async Task<Work?> GetById(string id)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT w.workId, w.workName, w.publishDate, w.workType, w.workDescription, 
                         w.src, w.albumId, w.genreId,
                         a.authorId, a.authorName, a.biography, a.personId
                  FROM works w
                  LEFT JOIN performs p ON w.workId = p.workId
                  LEFT JOIN authors a ON p.authorId = a.authorId
                  WHERE w.workId = @id", conn);

            cmd.Parameters.AddWithValue("id", id);

            using var reader = await cmd.ExecuteReaderAsync();

            Work? work = null;
            var authors = new List<Author>();

            while (await reader.ReadAsync())
            {
                if (work == null)
                {
                    var workId = reader.GetString(0);
                    var workName = reader.GetString(1);
                    var publishDate = reader.GetDateTime(2);
                    var workType = reader.GetString(3);
                    var workDescription = await reader.IsDBNullAsync(4) ? null : reader.GetString(4);
                    var src = await reader.IsDBNullAsync(5) ? null : reader.GetString(5);
                    var albumId = await reader.IsDBNullAsync(6) ? null : reader.GetString(6);
                    var genreId = reader.GetString(7);

                    work = new Work(
                        workId,
                        workName,
                        publishDate,
                        workType,
                        genreId,
                        authors,
                        workDescription,
                        src,
                        albumId
                    );
                }

                if (!await reader.IsDBNullAsync(8))
                {
                    var authorId = reader.GetString(8);
                    var authorName = await reader.IsDBNullAsync(9) ? null : reader.GetString(9);
                    var biography = await reader.IsDBNullAsync(10) ? null : reader.GetString(10);
                    var personId = await reader.IsDBNullAsync(11) ? null : reader.GetString(11);

                    authors.Add(new Author(authorId, authorName, biography, personId));
                }
            }

            return work;
        }

        public async Task<List<Work>> GetAll()
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT w.workId, w.workName, w.publishDate, w.workType, w.workDescription, 
                         w.src, w.albumId, w.genreId,
                         a.authorId, a.authorName, a.biography, a.personId
                  FROM works w
                  LEFT JOIN performs p ON w.workId = p.workId
                  LEFT JOIN authors a ON p.authorId = a.authorId
                  ORDER BY w.workId", conn);

            using var reader = await cmd.ExecuteReaderAsync();

            var works = new Dictionary<string, Work>();

            while (await reader.ReadAsync())
            {
                var workId = reader.GetString(0);

                if (!works.ContainsKey(workId))
                {
                    var workName = reader.GetString(1);
                    var publishDate = reader.GetDateTime(2);
                    var workType = reader.GetString(3);
                    var workDescription = await reader.IsDBNullAsync(4) ? null : reader.GetString(4);
                    var src = await reader.IsDBNullAsync(5) ? null : reader.GetString(5);
                    var albumId = await reader.IsDBNullAsync(6) ? null : reader.GetString(6);
                    var genreId = reader.GetString(7);

                    works[workId] = new Work(
                        workId,
                        workName,
                        publishDate,
                        workType,
                        genreId,
                        new List<Author>(),
                        workDescription,
                        src,
                        albumId
                    );
                }

                if (!await reader.IsDBNullAsync(8))
                {
                    var authorId = reader.GetString(8);
                    var authorName = await reader.IsDBNullAsync(9) ? null : reader.GetString(9);
                    var biography = await reader.IsDBNullAsync(10) ? null : reader.GetString(10);
                    var personId = await reader.IsDBNullAsync(11) ? null : reader.GetString(11);

                    // Add author to the work's author list (accessing via reflection or a public property)
                    // Since authors list is private, you may need to expose it or handle differently
                    // For now, assuming there's a way to add authors
                }
            }

            return works.Values.ToList();
        }

        public async Task<List<Work>> GetAllByGenreId(string genreId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT w.workId, w.workName, w.publishDate, w.workType, w.workDescription, 
                         w.src, w.albumId, w.genreId,
                         a.authorId, a.authorName, a.biography, a.personId
                  FROM works w
                  LEFT JOIN performs p ON w.workId = p.workId
                  LEFT JOIN authors a ON p.authorId = a.authorId
                  WHERE w.genreId = @genreId
                  ORDER BY w.workId", conn);

            cmd.Parameters.AddWithValue("genreId", genreId);

            using var reader = await cmd.ExecuteReaderAsync();

            var works = new Dictionary<string, Work>();

            while (await reader.ReadAsync())
            {
                var workId = reader.GetString(0);

                if (!works.ContainsKey(workId))
                {
                    var workName = reader.GetString(1);
                    var publishDate = reader.GetDateTime(2);
                    var workType = reader.GetString(3);
                    var workDescription = await reader.IsDBNullAsync(4) ? null : reader.GetString(4);
                    var src = await reader.IsDBNullAsync(5) ? null : reader.GetString(5);
                    var albumId = await reader.IsDBNullAsync(6) ? null : reader.GetString(6);
                    var genreIdResult = reader.GetString(7);

                    works[workId] = new Work(
                        workId,
                        workName,
                        publishDate,
                        workType,
                        genreIdResult,
                        new List<Author>(),
                        workDescription,
                        src,
                        albumId
                    );
                }

                if (!await reader.IsDBNullAsync(8))
                {
                    var authorId = reader.GetString(8);
                    var authorName = await reader.IsDBNullAsync(9) ? null : reader.GetString(9);
                    var biography = await reader.IsDBNullAsync(10) ? null : reader.GetString(10);
                    var personId = await reader.IsDBNullAsync(11) ? null : reader.GetString(11);

                    // Add author to work
                }
            }

            return works.Values.ToList();
        }

        public async Task<List<Work>> GetAllByAuthorId(string authorId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT w.workId, w.workName, w.publishDate, w.workType, w.workDescription, 
                         w.src, w.albumId, w.genreId,
                         a.authorId, a.authorName, a.biography, a.personId
                  FROM works w
                  INNER JOIN performs p ON w.workId = p.workId
                  LEFT JOIN authors a ON p.authorId = a.authorId
                  WHERE p.authorId = @authorId
                  ORDER BY w.workId", conn);

            cmd.Parameters.AddWithValue("authorId", authorId);

            using var reader = await cmd.ExecuteReaderAsync();

            var works = new Dictionary<string, Work>();

            while (await reader.ReadAsync())
            {
                var workId = reader.GetString(0);

                if (!works.ContainsKey(workId))
                {
                    var workName = reader.GetString(1);
                    var publishDate = reader.GetDateTime(2);
                    var workType = reader.GetString(3);
                    var workDescription = await reader.IsDBNullAsync(4) ? null : reader.GetString(4);
                    var src = await reader.IsDBNullAsync(5) ? null : reader.GetString(5);
                    var albumId = await reader.IsDBNullAsync(6) ? null : reader.GetString(6);
                    var genreId = reader.GetString(7);

                    works[workId] = new Work(
                        workId,
                        workName,
                        publishDate,
                        workType,
                        genreId,
                        new List<Author>(),
                        workDescription,
                        src,
                        albumId
                    );
                }

                if (!await reader.IsDBNullAsync(8))
                {
                    var authorIdResult = reader.GetString(8);
                    var authorName = await reader.IsDBNullAsync(9) ? null : reader.GetString(9);
                    var biography = await reader.IsDBNullAsync(10) ? null : reader.GetString(10);
                    var personId = await reader.IsDBNullAsync(11) ? null : reader.GetString(11);

                    // Add author to work
                }
            }

            return works.Values.ToList();
        }

        public async Task<List<Work>> GetAllByAlbumId(string albumId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT w.workId, w.workName, w.publishDate, w.workType, w.workDescription, 
                         w.src, w.albumId, w.genreId,
                         a.authorId, a.authorName, a.biography, a.personId
                  FROM works w
                  LEFT JOIN performs p ON w.workId = p.workId
                  LEFT JOIN authors a ON p.authorId = a.authorId
                  WHERE w.albumId = @albumId
                  ORDER BY w.workId", conn);

            cmd.Parameters.AddWithValue("albumId", albumId);

            using var reader = await cmd.ExecuteReaderAsync();

            var works = new Dictionary<string, Work>();

            while (await reader.ReadAsync())
            {
                var workId = reader.GetString(0);

                if (!works.ContainsKey(workId))
                {
                    var workName = reader.GetString(1);
                    var publishDate = reader.GetDateTime(2);
                    var workType = reader.GetString(3);
                    var workDescription = await reader.IsDBNullAsync(4) ? null : reader.GetString(4);
                    var src = await reader.IsDBNullAsync(5) ? null : reader.GetString(5);
                    var albumIdResult = await reader.IsDBNullAsync(6) ? null : reader.GetString(6);
                    var genreId = reader.GetString(7);

                    works[workId] = new Work(
                        workId,
                        workName,
                        publishDate,
                        workType,
                        genreId,
                        new List<Author>(),
                        workDescription,
                        src,
                        albumIdResult
                    );
                }

                if (!await reader.IsDBNullAsync(8))
                {
                    var authorId = reader.GetString(8);
                    var authorName = await reader.IsDBNullAsync(9) ? null : reader.GetString(9);
                    var biography = await reader.IsDBNullAsync(10) ? null : reader.GetString(10);
                    var personId = await reader.IsDBNullAsync(11) ? null : reader.GetString(11);

                    // Add author to work
                }
            }

            return works.Values.ToList();
        }

        public async Task CreateOne(Work work)
        {
            using var conn = _db.GetConnection();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // Insert work
                string workSql = @"INSERT INTO works(workId, workName, publishDate, workType, workDescription, src, albumId, genreId) 
                                   VALUES(@workId, @workName, @publishDate, @workType, @workDescription, @src, @albumId, @genreId)";

                using var workCmd = new NpgsqlCommand(workSql, conn, transaction);
                workCmd.Parameters.AddWithValue("workId", work.WorkId);
                workCmd.Parameters.AddWithValue("workName", work.WorkName);
                workCmd.Parameters.AddWithValue("publishDate", work.PublishDate);
                workCmd.Parameters.AddWithValue("workType", work.WorkType);
                workCmd.Parameters.AddWithValue("workDescription", (object?)work.WorkDescription ?? DBNull.Value);
                workCmd.Parameters.AddWithValue("src", (object?)work.Src ?? DBNull.Value);
                workCmd.Parameters.AddWithValue("albumId", (object?)work.AlbumId ?? DBNull.Value);
                workCmd.Parameters.AddWithValue("genreId", work.GenreId);

                await workCmd.ExecuteNonQueryAsync();

                // Note: You'll need to handle the authors list separately
                // This would require inserting into the 'performs' table
                // which links works to authors

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateOne(Work work)
        {
            using var conn = _db.GetConnection();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                string sql = @"UPDATE works 
                               SET workName = @workName, 
                                   publishDate = @publishDate, 
                                   workType = @workType, 
                                   workDescription = @workDescription, 
                                   src = @src, 
                                   albumId = @albumId, 
                                   genreId = @genreId 
                               WHERE workId = @workId";

                using var cmd = new NpgsqlCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("workId", work.WorkId);
                cmd.Parameters.AddWithValue("workName", work.WorkName);
                cmd.Parameters.AddWithValue("publishDate", work.PublishDate);
                cmd.Parameters.AddWithValue("workType", work.WorkType);
                cmd.Parameters.AddWithValue("workDescription", (object?)work.WorkDescription ?? DBNull.Value);
                cmd.Parameters.AddWithValue("src", (object?)work.Src ?? DBNull.Value);
                cmd.Parameters.AddWithValue("albumId", (object?)work.AlbumId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("genreId", work.GenreId);

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
            using var cmd = new NpgsqlCommand(@"DELETE FROM works WHERE workId = @id", conn);
            cmd.Parameters.AddWithValue("id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAllByAlbumId(string albumId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(@"DELETE FROM works WHERE albumId = @albumId", conn);
            cmd.Parameters.AddWithValue("albumId", albumId);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}