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

        public async Task<string?> GetVerificationCode(string userId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                $"SELECT verificationCode FROM users  WHERE userId = @id", conn);
            cmd.Parameters.AddWithValue("id", userId);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                if (await reader.IsDBNullAsync(reader.GetOrdinal("verificationCode")))
                {
                    return null;
                }
                return reader.GetString(reader.GetOrdinal("verificationCode"));
            }
            return null;
        }

        public async System.Threading.Tasks.Task CreateOne(User user)
        {
            using var conn = _db.GetConnection();
            await using var transaction = await conn.BeginTransactionAsync();
            try
            {
                // 1. Insert into 'people' table
                await using var insertPeopleCmd = new NpgsqlCommand(
                    @"INSERT INTO people (personId, personName, personSurname)
              VALUES (@id, @name, @surname)
              RETURNING personId;", conn, transaction);

                insertPeopleCmd.Parameters.AddWithValue("id", user.PersonId);
                insertPeopleCmd.Parameters.AddWithValue("name", user.Name);
                insertPeopleCmd.Parameters.AddWithValue("surname", user.Surname);

                await insertPeopleCmd.ExecuteNonQueryAsync();

                // 2. Insert into 'users' table
                await using var insertUserCmd = new NpgsqlCommand(
                    @"INSERT INTO users
              (userId, email, password, role, userStatus, contactVisible, reviewsVisible, verificationCode, personId)
              VALUES (@userId, @email, @password, @role, @status, @contactVisible, @reviewsVisible, @verificationCode, @personId);",
                      conn, transaction);

                insertUserCmd.Parameters.AddWithValue("userId", user.Id);
                insertUserCmd.Parameters.AddWithValue("email", user.Email);
                insertUserCmd.Parameters.AddWithValue("password", user.Password);
                insertUserCmd.Parameters.AddWithValue("role", user.Role);
                insertUserCmd.Parameters.AddWithValue("status", user.Status);
                insertUserCmd.Parameters.Add("contactVisible", NpgsqlTypes.NpgsqlDbType.Boolean).Value = (object)user.IsContactVisible ?? DBNull.Value;
                insertUserCmd.Parameters.Add("reviewsVisible", NpgsqlTypes.NpgsqlDbType.Boolean).Value = (object)user.AreReviewsVisible ?? DBNull.Value;
                insertUserCmd.Parameters.Add("verificationCode", NpgsqlTypes.NpgsqlDbType.Text).Value = (object)user.VerificationCode ?? DBNull.Value;
                insertUserCmd.Parameters.AddWithValue("personId", user.PersonId);

                await insertUserCmd.ExecuteNonQueryAsync();

                // 3. Insert qualifications if applicable
                if (user.Role == UserRole.EDITOR && user.Genres != null && user.Genres.Any())
                {
                    await using var insertQualCmd = new NpgsqlCommand(string.Empty, conn, transaction);
                    var sql = new System.Text.StringBuilder();

                    for(int i = 0; i < user.Genres.Count; ++i)
                    {
                        var genre = user.Genres[i];
                        sql.AppendLine($"INSERT INTO qualifications (userId, genreId) VALUES (@userId{i}, @genreId{i});");
                        insertQualCmd.Parameters.AddWithValue($"userId{i}", user.Id);
                        insertQualCmd.Parameters.AddWithValue($"genreId{i}", genre.Id);
                    }

                    if (sql.Length > 0)
                    {
                        insertQualCmd.CommandText = sql.ToString();
                        await insertQualCmd.ExecuteNonQueryAsync();
                    }
                }

                // 4. Commit transaction
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
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

        public async System.Threading.Tasks.Task UpdateOne(User user)
        {
            using var conn = _db.GetConnection();
            await using var transaction = await conn.BeginTransactionAsync();
            try
            {
                // 1. Update the 'people' table
                await using var peopleCmd = new NpgsqlCommand(
                    @"UPDATE people
              SET personName = @name,
                  personSurname = @surname
              WHERE personId = @personId;", conn, transaction);

                peopleCmd.Parameters.AddWithValue("name", user.Name);
                peopleCmd.Parameters.AddWithValue("surname", user.Surname);
                peopleCmd.Parameters.AddWithValue("personId", user.PersonId);

                await peopleCmd.ExecuteNonQueryAsync();

                // 2. Update the 'users' table
                await using var usersCmd = new NpgsqlCommand(
                    @"UPDATE users
              SET email = @email,
                  password = @password,
                  userStatus = @status,
                  contactVisible = @contactVisible,
                  reviewsVisible = @reviewsVisible,
                  verificationCode = @verificationCode
              WHERE userId = @userId;", conn, transaction);

                // Ensure the password is HASHED before updating if it was changed
                // You should handle hashing in your service layer before calling this method.
                // Assuming 'user.Password' is already the correct (hashed or plain, depending on your logic) value.

                usersCmd.Parameters.AddWithValue("email", user.Email);
                usersCmd.Parameters.AddWithValue("password", user.Password);
                usersCmd.Parameters.AddWithValue("status", user.Status);
                // Use NpgsqlDbType.Boolean for nullable bools
                usersCmd.Parameters.Add("contactVisible", NpgsqlTypes.NpgsqlDbType.Boolean).Value = (object)user.IsContactVisible ?? DBNull.Value;
                usersCmd.Parameters.Add("reviewsVisible", NpgsqlTypes.NpgsqlDbType.Boolean).Value = (object)user.AreReviewsVisible ?? DBNull.Value;
                // Use NpgsqlDbType.Text for nullable strings
                usersCmd.Parameters.Add("verificationCode", NpgsqlTypes.NpgsqlDbType.Text).Value = (object)user.VerificationCode ?? DBNull.Value;
                usersCmd.Parameters.AddWithValue("userId", user.Id);

                await usersCmd.ExecuteNonQueryAsync();

                // --- Handle 'qualifications' (Conditional on Role) ---

                // 3. Delete all existing qualifications for the user
                await using var deleteQualsCmd = new NpgsqlCommand(
                    @"DELETE FROM qualifications WHERE userId = @userId;", conn, transaction);
                deleteQualsCmd.Parameters.AddWithValue("userId", user.Id);
                await deleteQualsCmd.ExecuteNonQueryAsync();

                // 4. Insert new qualifications only if the user is an EDITOR and has genres
                if (user.Role == UserRole.EDITOR && user.Genres != null && user.Genres.Any())
                {
                    // You can use a single command to insert multiple rows using unnest
                    // or batch the INSERT statements. Batching is usually simpler and fast enough.

                    await using var insertQualCmd = new NpgsqlCommand(string.Empty, conn, transaction);
                    var sql = new System.Text.StringBuilder();

                    foreach (var genre in user.Genres)
                    {
                        // Ensure the Genre model has an Id property corresponding to genreId
                        sql.AppendLine($"INSERT INTO qualifications (userId, genreId) VALUES (@userId{genre.Id}, @genreId{genre.Id});");
                        insertQualCmd.Parameters.AddWithValue($"userId{genre.Id}", user.Id);
                        insertQualCmd.Parameters.AddWithValue($"genreId{genre.Id}", genre.Id);
                    }

                    if (sql.Length > 0)
                    {
                        insertQualCmd.CommandText = sql.ToString();
                        await insertQualCmd.ExecuteNonQueryAsync();
                    }
                }

                // 5. Commit the transaction if all commands succeeded
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                // 6. Rollback if any command failed
                await transaction.RollbackAsync();
                throw; // Re-throw the exception to be handled by the caller
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