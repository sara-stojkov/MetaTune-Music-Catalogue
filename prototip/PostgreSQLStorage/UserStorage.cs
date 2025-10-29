using Core.Model;
using Core.Storage;
using DotNetEnv;
using Npgsql;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;

namespace PostgreSQLStorage
{
    public class UserStorage(Database database) : IUserStorage
    {
        private readonly Database _db = database;

        public async Task<User?> GetByEmail(string email)
        {
            return await GetBy("email", email);
        }

        public async Task<User?> GetById(string id)
        {
            return await GetBy("userId", id);
        }
        private async Task<User?> GetBy(string column, string value)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                $"SELECT * FROM users NATURAL JOIN people WHERE {column} = @value", conn);
            cmd.Parameters.AddWithValue("value", value);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return new(
                        reader.GetString(reader.GetOrdinal("userId")),
                        reader.GetString(reader.GetOrdinal("personId")),
                        reader.GetString(reader.GetOrdinal("personName")),
                        reader.GetString(reader.GetOrdinal("personSurname")),
                        reader.GetString(reader.GetOrdinal("email")),
                        reader.GetString(reader.GetOrdinal("password")),
                        reader.GetString(reader.GetOrdinal("role")),
                        reader.GetString(reader.GetOrdinal("userStatus")),
                        await reader.IsDBNullAsync(reader.GetOrdinal("contactVisible")) ? 
                            null :
                            reader.GetBoolean(reader.GetOrdinal("contactVisible")),
                         await reader.IsDBNullAsync(reader.GetOrdinal("reviewsVisible")) ?
                            null :
                            reader.GetBoolean(reader.GetOrdinal("reviewsVisible")),
                         await reader.IsDBNullAsync(reader.GetOrdinal("verificationCode")) ?
                            null :
                            reader.GetString(reader.GetOrdinal("verificationCode"))
                    );
            return null;
        }
    }
}
