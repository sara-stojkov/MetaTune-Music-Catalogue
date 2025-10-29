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
                    var workTypeString = reader.GetString(3);
                    var workType = Enum.Parse<WorkType>(workTypeString, true);
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

            var worksDict = new Dictionary<string, (Work work, List<Author> authors)>();

            while (await reader.ReadAsync())
            {
                var workId = reader.GetString(0);

                if (!worksDict.ContainsKey(workId))
                {
                    var workName = reader.GetString(1);
                    var publishDate = reader.GetDateTime(2);
                    var workTypeString = reader.GetString(3);
                    var workType = Enum.Parse<WorkType>(workTypeString, true);
                    var workDescription = await reader.IsDBNullAsync(4) ? null : reader.GetString(4);
                    var src = await reader.IsDBNullAsync(5) ? null : reader.GetString(5);
                    var albumId = await reader.IsDBNullAsync(6) ? null : reader.GetString(6);
                    var genreId = reader.GetString(7);

                    var authors = new List<Author>();
                    var work = new Work(
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

                    worksDict[workId] = (work, authors);
                }

                if (!await reader.IsDBNullAsync(8))
                {
                    var authorId = reader.GetString(8);
                    var authorName = await reader.IsDBNullAsync(9) ? null : reader.GetString(9);
                    var biography = await reader.IsDBNullAsync(10) ? null : reader.GetString(10);
                    var personId = await reader.IsDBNullAsync(11) ? null : reader.GetString(11);

                    var author = new Author(authorId, authorName, biography, personId);
                    worksDict[workId].authors.Add(author);
                }
            }

            return worksDict.Values.Select(x => x.work).ToList();
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

            var worksDict = new Dictionary<string, (Work work, List<Author> authors)>();

            while (await reader.ReadAsync())
            {
                var workId = reader.GetString(0);

                if (!worksDict.ContainsKey(workId))
                {
                    var workName = reader.GetString(1);
                    var publishDate = reader.GetDateTime(2);
                    var workTypeString = reader.GetString(3);
                    var workType = Enum.Parse<WorkType>(workTypeString, true);
                    var workDescription = await reader.IsDBNullAsync(4) ? null : reader.GetString(4);
                    var src = await reader.IsDBNullAsync(5) ? null : reader.GetString(5);
                    var albumId = await reader.IsDBNullAsync(6) ? null : reader.GetString(6);
                    var genreIdResult = reader.GetString(7);

                    var authors = new List<Author>();
                    var work = new Work(
                        workId,
                        workName,
                        publishDate,
                        workType,
                        genreIdResult,
                        authors,
                        workDescription,
                        src,
                        albumId
                    );

                    worksDict[workId] = (work, authors);
                }

                if (!await reader.IsDBNullAsync(8))
                {
                    var authorId = reader.GetString(8);
                    var authorName = await reader.IsDBNullAsync(9) ? null : reader.GetString(9);
                    var biography = await reader.IsDBNullAsync(10) ? null : reader.GetString(10);
                    var personId = await reader.IsDBNullAsync(11) ? null : reader.GetString(11);

                    var author = new Author(authorId, authorName, biography, personId);
                    worksDict[workId].authors.Add(author);
                }
            }

            return worksDict.Values.Select(x => x.work).ToList();
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

            var worksDict = new Dictionary<string, (Work work, List<Author> authors)>();

            while (await reader.ReadAsync())
            {
                var workId = reader.GetString(0);

                if (!worksDict.ContainsKey(workId))
                {
                    var workName = reader.GetString(1);
                    var publishDate = reader.GetDateTime(2);
                    var workTypeString = reader.GetString(3);
                    var workType = Enum.Parse<WorkType>(workTypeString, true);
                    var workDescription = await reader.IsDBNullAsync(4) ? null : reader.GetString(4);
                    var src = await reader.IsDBNullAsync(5) ? null : reader.GetString(5);
                    var albumId = await reader.IsDBNullAsync(6) ? null : reader.GetString(6);
                    var genreId = reader.GetString(7);

                    var authors = new List<Author>();
                    var work = new Work(
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

                    worksDict[workId] = (work, authors);
                }

                if (!await reader.IsDBNullAsync(8))
                {
                    var authorIdResult = reader.GetString(8);
                    var authorName = await reader.IsDBNullAsync(9) ? null : reader.GetString(9);
                    var biography = await reader.IsDBNullAsync(10) ? null : reader.GetString(10);
                    var personId = await reader.IsDBNullAsync(11) ? null : reader.GetString(11);

                    var author = new Author(authorIdResult, authorName, biography, personId);
                    worksDict[workId].authors.Add(author);
                }
            }

            return worksDict.Values.Select(x => x.work).ToList();
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

            var worksDict = new Dictionary<string, (Work work, List<Author> authors)>();

            while (await reader.ReadAsync())
            {
                var workId = reader.GetString(0);

                if (!worksDict.ContainsKey(workId))
                {
                    var workName = reader.GetString(1);
                    var publishDate = reader.GetDateTime(2);
                    var workTypeString = reader.GetString(3);
                    var workType = Enum.Parse<WorkType>(workTypeString, true);
                    var workDescription = await reader.IsDBNullAsync(4) ? null : reader.GetString(4);
                    var src = await reader.IsDBNullAsync(5) ? null : reader.GetString(5);
                    var albumIdResult = await reader.IsDBNullAsync(6) ? null : reader.GetString(6);
                    var genreId = reader.GetString(7);

                    var authors = new List<Author>();
                    var work = new Work(
                        workId,
                        workName,
                        publishDate,
                        workType,
                        genreId,
                        authors,
                        workDescription,
                        src,
                        albumIdResult
                    );

                    worksDict[workId] = (work, authors);
                }

                if (!await reader.IsDBNullAsync(8))
                {
                    var authorId = reader.GetString(8);
                    var authorName = await reader.IsDBNullAsync(9) ? null : reader.GetString(9);
                    var biography = await reader.IsDBNullAsync(10) ? null : reader.GetString(10);
                    var personId = await reader.IsDBNullAsync(11) ? null : reader.GetString(11);

                    var author = new Author(authorId, authorName, biography, personId);
                    worksDict[workId].authors.Add(author);
                }
            }

            return worksDict.Values.Select(x => x.work).ToList();
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
                workCmd.Parameters.AddWithValue("workType", work.WorkType.ToString());
                workCmd.Parameters.AddWithValue("workDescription", (object?)work.WorkDescription ?? DBNull.Value);
                workCmd.Parameters.AddWithValue("src", (object?)work.Src ?? DBNull.Value);
                workCmd.Parameters.AddWithValue("albumId", (object?)work.AlbumId ?? DBNull.Value);
                workCmd.Parameters.AddWithValue("genreId", work.GenreId);

                await workCmd.ExecuteNonQueryAsync();

                // Insert author relationships into performs table
                if (work.Authors != null && work.Authors.Count > 0)
                {
                    string performsSql = @"INSERT INTO performs(workId, authorId) VALUES(@workId, @authorId)";

                    foreach (var author in work.Authors)
                    {
                        using var performsCmd = new NpgsqlCommand(performsSql, conn, transaction);
                        performsCmd.Parameters.AddWithValue("workId", work.WorkId);
                        performsCmd.Parameters.AddWithValue("authorId", author.AuthorId);
                        await performsCmd.ExecuteNonQueryAsync();
                    }
                }

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
                cmd.Parameters.AddWithValue("workType", work.WorkType.ToString());
                cmd.Parameters.AddWithValue("workDescription", (object?)work.WorkDescription ?? DBNull.Value);
                cmd.Parameters.AddWithValue("src", (object?)work.Src ?? DBNull.Value);
                cmd.Parameters.AddWithValue("albumId", (object?)work.AlbumId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("genreId", work.GenreId);

                await cmd.ExecuteNonQueryAsync();

                // Update author relationships
                // First, delete existing relationships
                string deletePerformsSql = @"DELETE FROM performs WHERE workId = @workId";
                using var deleteCmd = new NpgsqlCommand(deletePerformsSql, conn, transaction);
                deleteCmd.Parameters.AddWithValue("workId", work.WorkId);
                await deleteCmd.ExecuteNonQueryAsync();

                // Then, insert new relationships
                if (work.Authors != null && work.Authors.Count > 0)
                {
                    string insertPerformsSql = @"INSERT INTO performs(workId, authorId) VALUES(@workId, @authorId)";

                    foreach (var author in work.Authors)
                    {
                        using var insertCmd = new NpgsqlCommand(insertPerformsSql, conn, transaction);
                        insertCmd.Parameters.AddWithValue("workId", work.WorkId);
                        insertCmd.Parameters.AddWithValue("authorId", author.AuthorId);
                        await insertCmd.ExecuteNonQueryAsync();
                    }
                }

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
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // First delete from performs table (foreign key constraint)
                string deletePerformsSql = @"DELETE FROM performs WHERE workId = @id";
                using var deletePerformsCmd = new NpgsqlCommand(deletePerformsSql, conn, transaction);
                deletePerformsCmd.Parameters.AddWithValue("id", id);
                await deletePerformsCmd.ExecuteNonQueryAsync();

                // Then delete the work
                string deleteWorkSql = @"DELETE FROM works WHERE workId = @id";
                using var deleteWorkCmd = new NpgsqlCommand(deleteWorkSql, conn, transaction);
                deleteWorkCmd.Parameters.AddWithValue("id", id);
                await deleteWorkCmd.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteAllByAlbumId(string albumId)
        {
            using var conn = _db.GetConnection();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // First delete from performs table
                string deletePerformsSql = @"DELETE FROM performs WHERE workId IN (SELECT workId FROM works WHERE albumId = @albumId)";
                using var deletePerformsCmd = new NpgsqlCommand(deletePerformsSql, conn, transaction);
                deletePerformsCmd.Parameters.AddWithValue("albumId", albumId);
                await deletePerformsCmd.ExecuteNonQueryAsync();

                // Then delete the works
                string deleteWorksSql = @"DELETE FROM works WHERE albumId = @albumId";
                using var deleteWorksCmd = new NpgsqlCommand(deleteWorksSql, conn, transaction);
                deleteWorksCmd.Parameters.AddWithValue("albumId", albumId);
                await deleteWorksCmd.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}