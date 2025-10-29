using Core.Model;
using Core.Storage;
using DotNetEnv;
using Npgsql;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;
using Task = System.Threading.Tasks.Task;

namespace PostgreSQLStorage
{
    public class GenreStorage(Database database) : IGenreStorage
    {
        private readonly Database _db = database;

        public async Task<Genre?> GetById(string id)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT genreId, genreName, genreDescription, parentGenreId 
                  FROM genres 
                  WHERE genreId = @id", conn);

            cmd.Parameters.AddWithValue("id", id);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var genreId = reader.GetString(0);
                var genreName = reader.GetString(1);
                var genreDescription = await reader.IsDBNullAsync(2) ? string.Empty : reader.GetString(2);
                var parentGenreId = reader.GetString(3);

                return new Genre(genreId, genreName, genreDescription, parentGenreId);
            }

            return null;
        }

        public async Task<List<Genre>> GetAll()
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT genreId, genreName, genreDescription, parentGenreId 
                  FROM genres 
                  ORDER BY genreName", conn);

            using var reader = await cmd.ExecuteReaderAsync();

            var genres = new List<Genre>();

            while (await reader.ReadAsync())
            {
                var genreId = reader.GetString(0);
                var genreName = reader.GetString(1);
                var genreDescription = await reader.IsDBNullAsync(2) ? string.Empty : reader.GetString(2);
                var parentGenreId = reader.GetString(3);

                genres.Add(new Genre(genreId, genreName, genreDescription, parentGenreId));
            }

            return genres;
        }

        public async Task CreateOne(Genre genre)
        {
            using var conn = _db.GetConnection();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                string sql = @"INSERT INTO genres(genreId, genreName, genreDescription, parentGenreId) 
                               VALUES(@genreId, @genreName, @genreDescription, @parentGenreId)";

                using var cmd = new NpgsqlCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("genreId", genre.Id);
                cmd.Parameters.AddWithValue("genreName", genre.Name);
                cmd.Parameters.AddWithValue("genreDescription", string.IsNullOrEmpty(genre.Description) ? DBNull.Value : genre.Description);
                cmd.Parameters.AddWithValue("parentGenreId", genre.ParentGenreId);

                await cmd.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateOne(Genre genre)
        {
            using var conn = _db.GetConnection();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                string sql = @"UPDATE genres 
                               SET genreName = @genreName, 
                                   genreDescription = @genreDescription,
                                   parentGenreId = @parentGenreId 
                               WHERE genreId = @genreId";

                using var cmd = new NpgsqlCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("genreId", genre.Id);
                cmd.Parameters.AddWithValue("genreName", genre.Name);
                cmd.Parameters.AddWithValue("genreDescription", string.IsNullOrEmpty(genre.Description) ? DBNull.Value : genre.Description);
                cmd.Parameters.AddWithValue("parentGenreId", genre.ParentGenreId);

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
            string sql = @"DELETE FROM genres WHERE genreId = @id";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<Genre>> GetEditorsGenres(string editorId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT g.genreId, g.genreName, g.genreDescription, g.parentGenreId 
                  FROM qualifications q
                  INNER JOIN genres g ON q.genreId = g.genreId
                  WHERE q.userId = @id
                  ORDER BY g.genreName", conn);

            cmd.Parameters.AddWithValue("id", editorId);

            using var reader = await cmd.ExecuteReaderAsync();
            var genres = new List<Genre>();

            while (await reader.ReadAsync())
            {
                var genreId = reader.GetString(0);
                var genreName = reader.GetString(1);
                var genreDescription = await reader.IsDBNullAsync(2) ? string.Empty : reader.GetString(2);
                var parentGenreId = reader.GetString(3);

                genres.Add(new Genre(genreId, genreName, genreDescription, parentGenreId));
            }

            return genres;
        }

        public async Task<List<Genre>> GetAllSubGenres(string genreId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT genreId, genreName, genreDescription, parentGenreId 
                  FROM genres 
                  WHERE parentGenreId = @genreId
                  ORDER BY genreName", conn);

            cmd.Parameters.AddWithValue("genreId", genreId);

            using var reader = await cmd.ExecuteReaderAsync();
            var subGenres = new List<Genre>();

            while (await reader.ReadAsync())
            {
                var subGenreId = reader.GetString(0);
                var genreName = reader.GetString(1);
                var genreDescription = await reader.IsDBNullAsync(2) ? string.Empty : reader.GetString(2);
                var parentGenreId = reader.GetString(3);

                subGenres.Add(new Genre(subGenreId, genreName, genreDescription, parentGenreId));
            }

            return subGenres;
        }
    }
}