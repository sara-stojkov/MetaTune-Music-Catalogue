using Core.Model;
using Core.Storage;
using DotNetEnv;
using Npgsql;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;

namespace PostgreSQLStorage
{
    public class GenreStorage(Database database) : IGenreStorage
    {
        private readonly Database _db = database;
        public async Task<List<Genre>> GetEditorsGenres(string editorId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                $"SELECT genreId, name, description, parentGenreId " + // Explicitly list columns for clarity/safety
                $"FROM qualifications NATURAL JOIN genres WHERE userId = @id", conn);
            cmd.Parameters.AddWithValue("id", editorId);

            // Using a dictionary to hold all fetched genres for quick lookup and structure building
            var allGenres = new Dictionary<string, Genre>();
            // Using a list to hold the parent IDs for later grouping
            var genreParents = new Dictionary<string, string>(); // Key: genreId, Value: parentGenreId

            using var reader = await cmd.ExecuteReaderAsync();

            // 1. Read all rows and create flat Genre objects
            while (await reader.ReadAsync())
            {
                string id = reader["genreId"]?.ToString() ?? string.Empty;
                string name = reader["name"]?.ToString() ?? string.Empty;
                // Npgsql supports getting nullable columns with IsDBNull
                string? description = await reader.IsDBNullAsync(reader.GetOrdinal("description"))
                    ? string.Empty
                    : reader["description"].ToString();

                string? parentId = await reader.IsDBNullAsync(reader.GetOrdinal("parentGenreId"))
                    ? null
                    : reader["parentGenreId"].ToString();

                // Create the Genre object
                var genre = new Genre(id, name, description!);
                allGenres.Add(id, genre);

                // Record the parent relationship if one exists
                if (parentId != null)
                {
                    genreParents.Add(id, parentId);
                }
            }

            // 2. Build the hierarchy (subgenres)
            foreach (var entry in genreParents)
            {
                string subGenreId = entry.Key;
                string parentGenreId = entry.Value;

                // Check if both the subgenre and its parent were returned in the query results
                // An editor might be qualified for a subgenre but not its parent, 
                // or vice-versa, but we only process what the query returned.
                if (allGenres.ContainsKey(parentGenreId) && allGenres.ContainsKey(subGenreId))
                {
                    // Add the subgenre to its parent's SubGenres list
                    allGenres[parentGenreId].SubGenres.Add(allGenres[subGenreId]);
                }
            }

            // 3. Filter for top-level genres (those with no parent or whose parent wasn't fetched)
            var topLevelGenres = new List<Genre>();

            foreach (var genre in allGenres.Values)
            {
                // A genre is top-level for this editor if its ID is NOT a value 
                // in the genreParents dictionary (meaning it was not recorded as a subgenre of any other genre fetched)
                if (!genreParents.ContainsKey(genre.Id))
                {
                    topLevelGenres.Add(genre);
                }
            }

            return topLevelGenres;
        }
    }
}
