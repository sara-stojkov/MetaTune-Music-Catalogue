using Core.Model;
using Core.Storage;
using Npgsql;
using Task = System.Threading.Tasks.Task;

namespace PostgreSQLStorage
{
    public class ContributorStorage : IContributorStorage
    {
        private readonly Database _db;

        public ContributorStorage(Database db)
        {
            _db = db;
        }

        public async Task<List<Contributor>> GetAll()
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT contributionType, personId, workId 
                  FROM contributors 
                  ORDER BY workId, personId", conn);

            using var reader = await cmd.ExecuteReaderAsync();

            var contributors = new List<Contributor>();

            while (await reader.ReadAsync())
            {
                var contributionType = reader.GetString(0);
                var personId = reader.GetString(1);
                var workId = reader.GetString(2);

                contributors.Add(new Contributor(contributionType, personId, workId));
            }

            return contributors;
        }

        public async Task<List<Contributor>> GetAllByWorkId(string workId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT contributionType, personId, workId 
                  FROM contributors 
                  WHERE workId = @workId
                  ORDER BY personId", conn);

            cmd.Parameters.AddWithValue("workId", workId);

            using var reader = await cmd.ExecuteReaderAsync();

            var contributors = new List<Contributor>();

            while (await reader.ReadAsync())
            {
                var contributionType = reader.GetString(0);
                var personId = reader.GetString(1);
                var workIdResult = reader.GetString(2);

                contributors.Add(new Contributor(contributionType, personId, workIdResult));
            }

            return contributors;
        }

        public async Task CreateOne(Contributor contributor)
        {
            using var conn = _db.GetConnection();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                string sql = @"INSERT INTO contributors(contributionType, personId, workId) 
                               VALUES(@contributionType, @personId, @workId)";

                using var cmd = new NpgsqlCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("contributionType", contributor.ContributionType);
                cmd.Parameters.AddWithValue("personId", contributor.PersonId);
                cmd.Parameters.AddWithValue("workId", contributor.WorkId);

                await cmd.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateOne(Contributor contributor)
        {
            using var conn = _db.GetConnection();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                string sql = @"UPDATE contributors 
                               SET contributionType = @contributionType 
                               WHERE personId = @personId AND workId = @workId";

                using var cmd = new NpgsqlCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("contributionType", contributor.ContributionType);
                cmd.Parameters.AddWithValue("personId", contributor.PersonId);
                cmd.Parameters.AddWithValue("workId", contributor.WorkId);

                await cmd.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteById(string personId, string workId)
        {
            using var conn = _db.GetConnection();
            string sql = @"DELETE FROM contributors 
                           WHERE personId = @personId AND workId = @workId";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("personId", personId);
            cmd.Parameters.AddWithValue("workId", workId);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}