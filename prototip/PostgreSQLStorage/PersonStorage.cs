using Core.Model;
using Core.Storage;
using Npgsql;
using Task = System.Threading.Tasks.Task;

namespace PostgreSQLStorage
{
    public class PersonStorage : IPersonStorage
    {
        private readonly Database _db;

        public PersonStorage(Database db)
        {
            _db = db;
        }

        public async Task<Person?> GetById(string id)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT personId, personName, personSurname 
                  FROM people 
                  WHERE personId = @id", conn);

            cmd.Parameters.AddWithValue("id", id);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var personId = reader.GetString(0);
                var personName = reader.GetString(1);
                var personSurname = reader.GetString(2);

                return new Person(personId, personName, personSurname);
            }

            return null;
        }

        public async Task<List<Person>> GetAll()
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT personId, personName, personSurname 
                  FROM people 
                  ORDER BY personSurname, personName", conn);

            using var reader = await cmd.ExecuteReaderAsync();

            var people = new List<Person>();

            while (await reader.ReadAsync())
            {
                var personId = reader.GetString(0);
                var personName = reader.GetString(1);
                var personSurname = reader.GetString(2);

                people.Add(new Person(personId, personName, personSurname));
            }

            return people;
        }

        public async Task CreateOne(Person person)
        {
            using var conn = _db.GetConnection();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                string sql = @"INSERT INTO people(personId, personName, personSurname) 
                               VALUES(@personId, @personName, @personSurname)";

                using var cmd = new NpgsqlCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("personId", person.PersonId);
                cmd.Parameters.AddWithValue("personName", person.PersonName);
                cmd.Parameters.AddWithValue("personSurname", person.PersonSurname);

                await cmd.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateOne(Person person)
        {
            using var conn = _db.GetConnection();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                string sql = @"UPDATE people 
                               SET personName = @personName, 
                                   personSurname = @personSurname 
                               WHERE personId = @personId";

                using var cmd = new NpgsqlCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("personId", person.PersonId);
                cmd.Parameters.AddWithValue("personName", person.PersonName);
                cmd.Parameters.AddWithValue("personSurname", person.PersonSurname);

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
            string sql = @"DELETE FROM people WHERE personId = @id";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}