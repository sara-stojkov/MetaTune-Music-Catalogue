using Core.Model;
using Core.Storage;
using Npgsql;
using Task = System.Threading.Tasks.Task;

namespace PostgreSQLStorage
{
    public class AuthorStorage : IAuthorStorage
    {
        private readonly Database _db;


        public AuthorStorage(Database db)
        {
            _db = db;
        }

        public async Task<Author?> GetById(string id)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT authorId, authorName, biography, personId 
                  FROM authors 
                  WHERE authorId = @id", conn);

            cmd.Parameters.AddWithValue("id", id);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var authorId = reader.GetString(0);
                var authorName = await reader.IsDBNullAsync(1) ? null : reader.GetString(1);
                var biography = await reader.IsDBNullAsync(2) ? null : reader.GetString(2);
                var personId = await reader.IsDBNullAsync(3) ? null : reader.GetString(3);

                return new Author(authorId, authorName, biography, personId);
            }

            return null;
        }

        public async Task<List<Author>> GetAll(AuthorFilter filter)
        {
            using var conn = _db.GetConnection();
            string sql = filter switch
            {
                AuthorFilter.Group => @"SELECT DISTINCT a.authorId, a.authorName, a.biography, a.personId 
                                        FROM authors a
                                        WHERE a.personId IS NULL
                                        ORDER BY a.authorId",

                AuthorFilter.Solo => @"
                                        SELECT a.authorId, a.authorName, a.biography, a.personId 
                                        FROM authors a
                                        WHERE a.personId IS NOT NULL
                                          AND a.authorId NOT IN (
                                                SELECT groupId FROM members
                                                UNION
                                                SELECT memberId FROM members
                                            )
                                        ORDER BY a.authorId",


                _ => @"SELECT authorId, authorName, biography, personId 
                       FROM authors 
                       ORDER BY authorId"
            };

            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            var authors = new List<Author>();

            while (await reader.ReadAsync())
            {
                var authorId = reader.GetString(0);
                var authorName = await reader.IsDBNullAsync(1) ? null : reader.GetString(1);
                var biography = await reader.IsDBNullAsync(2) ? null : reader.GetString(2);
                var personId = await reader.IsDBNullAsync(3) ? null : reader.GetString(3);

                authors.Add(new Author(authorId, authorName, biography, personId));
            }

            return authors;
        }

        public async Task CreateOne(Author author, Person? person = null)
        {
            using var conn = _db.GetConnection();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // Ako postoji Person, prvo kreiraj Person
                if (person != null && author.PersonId != null)
                {
                    string psql = @"INSERT INTO people(personId, personName, personSurname) 
                               VALUES(@personId, @personName, @personSurname)";

                    using var pcmd = new NpgsqlCommand(psql, conn, transaction);
                    pcmd.Parameters.AddWithValue("personId", person.PersonId);
                    pcmd.Parameters.AddWithValue("personName", person.PersonName);
                    pcmd.Parameters.AddWithValue("personSurname", person.PersonSurname);

                    await pcmd.ExecuteNonQueryAsync();
                }

                string sql = @"INSERT INTO authors(authorId, authorName, biography, personId) 
                               VALUES(@authorId, @authorName, @biography, @personId)";

                using var cmd = new NpgsqlCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("authorId", author.AuthorId);
                cmd.Parameters.AddWithValue("authorName", (object?)author.AuthorName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("biography", (object?)author.Biography ?? DBNull.Value);
                cmd.Parameters.AddWithValue("personId", (object?)author.PersonId ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateOne(Author author)
        {
            using var conn = _db.GetConnection();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                string sql = @"UPDATE authors 
                               SET authorName = @authorName, 
                                   biography = @biography, 
                                   personId = @personId 
                               WHERE authorId = @authorId";

                using var cmd = new NpgsqlCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("authorId", author.AuthorId);
                cmd.Parameters.AddWithValue("authorName", (object?)author.AuthorName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("biography", (object?)author.Biography ?? DBNull.Value);
                cmd.Parameters.AddWithValue("personId", (object?)author.PersonId ?? DBNull.Value);

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
            string sql = @"DELETE FROM authors WHERE authorId = @id";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<Author>> GetAllAuthorsByGenreId(string genreId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT a.authorId, a.authorName, a.biography, a.personId 
                  FROM authors a
                  INNER JOIN belongs b ON a.authorId = b.authorId
                  WHERE b.genreId = @genreId
                  ORDER BY a.authorName", conn);

            cmd.Parameters.AddWithValue("genreId", genreId);

            using var reader = await cmd.ExecuteReaderAsync();

            var authors = new List<Author>();

            while (await reader.ReadAsync())
            {
                var authorId = reader.GetString(0);
                var authorName = await reader.IsDBNullAsync(1) ? null : reader.GetString(1);
                var biography = await reader.IsDBNullAsync(2) ? null : reader.GetString(2);
                var personId = await reader.IsDBNullAsync(3) ? null : reader.GetString(3);

                authors.Add(new Author(authorId, authorName, biography, personId));
            }

            return authors;
        }
    }
}