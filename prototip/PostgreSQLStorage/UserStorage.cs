using Core.Model;
using Core.Storage;
using DotNetEnv;
using Npgsql;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;
using Task = System.Threading.Tasks.Task;

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

        public async Task<List<User>> GetAll()
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT u.userId, u.personId, p.personName, p.personSurname, 
                         u.email, u.password, u.role, u.userStatus, 
                         u.contactVisible, u.reviewsVisible, u.verificationCode
                  FROM users u
                  INNER JOIN people p ON u.personId = p.personId
                  WHERE u.userStatus = @status
                  ORDER BY p.personSurname, p.personName", conn);

            cmd.Parameters.AddWithValue("status", UserStatus.ACTIVE);

            using var reader = await cmd.ExecuteReaderAsync();
            var users = new List<User>();

            while (await reader.ReadAsync())
            {
                users.Add(CreateUserFromReader(reader));
            }

            return users;
        }

        public async Task<List<User>> GetAllByRole(string role)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT u.userId, u.personId, p.personName, p.personSurname, 
                         u.email, u.password, u.role, u.userStatus, 
                         u.contactVisible, u.reviewsVisible, u.verificationCode
                  FROM users u
                  INNER JOIN people p ON u.personId = p.personId
                  WHERE u.role = @role AND u.userStatus = @status
                  ORDER BY p.personSurname, p.personName", conn);

            cmd.Parameters.AddWithValue("role", role);
            cmd.Parameters.AddWithValue("status", UserStatus.ACTIVE);

            using var reader = await cmd.ExecuteReaderAsync();
            var users = new List<User>();

            while (await reader.ReadAsync())
            {
                users.Add(CreateUserFromReader(reader));
            }

            return users;
        }

        public async Task CreateOne(User user)
        {
            using var conn = _db.GetConnection();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // First, create the person
                string personSql = @"INSERT INTO people(personId, personName, personSurname) 
                                     VALUES(@personId, @personName, @personSurname)";

                using var personCmd = new NpgsqlCommand(personSql, conn, transaction);
                personCmd.Parameters.AddWithValue("personId", user.PersonId);
                personCmd.Parameters.AddWithValue("personName", user.Name);
                personCmd.Parameters.AddWithValue("personSurname", user.Surname);
                await personCmd.ExecuteNonQueryAsync();

                // Then, create the user
                string userSql = @"INSERT INTO users(userId, email, password, role, personId, userStatus, 
                                                     contactVisible, reviewsVisible, verificationCode) 
                                   VALUES(@userId, @email, @password, @role, @personId, @userStatus, 
                                          @contactVisible, @reviewsVisible, @verificationCode)";

                using var userCmd = new NpgsqlCommand(userSql, conn, transaction);
                userCmd.Parameters.AddWithValue("userId", user.Id);
                userCmd.Parameters.AddWithValue("email", user.Email);
                userCmd.Parameters.AddWithValue("password", user.Password);
                userCmd.Parameters.AddWithValue("role", user.Role);
                userCmd.Parameters.AddWithValue("personId", user.PersonId);
                userCmd.Parameters.AddWithValue("userStatus", user.Status);
                userCmd.Parameters.AddWithValue("contactVisible", (object?)user.IsContactVisible ?? DBNull.Value);
                userCmd.Parameters.AddWithValue("reviewsVisible", (object?)user.AreReviewsVisible ?? DBNull.Value);
                userCmd.Parameters.AddWithValue("verificationCode", (object?)user.VerificationCode ?? DBNull.Value);

                await userCmd.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateOne(User user)
        {
            using var conn = _db.GetConnection();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // Update person
                string personSql = @"UPDATE people 
                                     SET personName = @personName, 
                                         personSurname = @personSurname 
                                     WHERE personId = @personId";

                using var personCmd = new NpgsqlCommand(personSql, conn, transaction);
                personCmd.Parameters.AddWithValue("personId", user.PersonId);
                personCmd.Parameters.AddWithValue("personName", user.Name);
                personCmd.Parameters.AddWithValue("personSurname", user.Surname);
                await personCmd.ExecuteNonQueryAsync();

                // Update user
                string userSql = @"UPDATE users 
                                   SET email = @email, 
                                       password = @password, 
                                       userStatus = @userStatus, 
                                       contactVisible = @contactVisible, 
                                       reviewsVisible = @reviewsVisible, 
                                       verificationCode = @verificationCode 
                                   WHERE userId = @userId";

                using var userCmd = new NpgsqlCommand(userSql, conn, transaction);
                userCmd.Parameters.AddWithValue("userId", user.Id);
                userCmd.Parameters.AddWithValue("email", user.Email);
                userCmd.Parameters.AddWithValue("password", user.Password);
                userCmd.Parameters.AddWithValue("userStatus", user.Status);
                userCmd.Parameters.AddWithValue("contactVisible", (object?)user.IsContactVisible ?? DBNull.Value);
                userCmd.Parameters.AddWithValue("reviewsVisible", (object?)user.AreReviewsVisible ?? DBNull.Value);
                userCmd.Parameters.AddWithValue("verificationCode", (object?)user.VerificationCode ?? DBNull.Value);

                await userCmd.ExecuteNonQueryAsync();
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
                // Get personId first
                string getPersonSql = @"SELECT personId FROM users WHERE userId = @userId";
                using var getCmd = new NpgsqlCommand(getPersonSql, conn, transaction);
                getCmd.Parameters.AddWithValue("userId", id);
                var personId = await getCmd.ExecuteScalarAsync() as string;

                // Delete user
                string deleteUserSql = @"DELETE FROM users WHERE userId = @userId";
                using var deleteUserCmd = new NpgsqlCommand(deleteUserSql, conn, transaction);
                deleteUserCmd.Parameters.AddWithValue("userId", id);
                await deleteUserCmd.ExecuteNonQueryAsync();

                // Delete person
                if (personId != null)
                {
                    string deletePersonSql = @"DELETE FROM people WHERE personId = @personId";
                    using var deletePersonCmd = new NpgsqlCommand(deletePersonSql, conn, transaction);
                    deletePersonCmd.Parameters.AddWithValue("personId", personId);
                    await deletePersonCmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<User?> GetBy(string column, string value)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                $"SELECT * FROM users NATURAL JOIN people WHERE {column} = @value", conn);
            cmd.Parameters.AddWithValue("value", value);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return CreateUserFromReader(reader);
            return null;
        }

        private User CreateUserFromReader(NpgsqlDataReader reader)
        {
            return new(
                reader.GetString(reader.GetOrdinal("userId")),
                reader.GetString(reader.GetOrdinal("personId")),
                reader.GetString(reader.GetOrdinal("personName")),
                reader.GetString(reader.GetOrdinal("personSurname")),
                reader.GetString(reader.GetOrdinal("email")),
                reader.GetString(reader.GetOrdinal("password")),
                reader.GetString(reader.GetOrdinal("role")),
                reader.GetString(reader.GetOrdinal("userStatus")),
                reader.IsDBNull(reader.GetOrdinal("contactVisible")) ?
                    null :
                    reader.GetBoolean(reader.GetOrdinal("contactVisible")),
                reader.IsDBNull(reader.GetOrdinal("reviewsVisible")) ?
                    null :
                    reader.GetBoolean(reader.GetOrdinal("reviewsVisible")),
                reader.IsDBNull(reader.GetOrdinal("verificationCode")) ?
                    null :
                    reader.GetString(reader.GetOrdinal("verificationCode"))
            );
        }
    }
}