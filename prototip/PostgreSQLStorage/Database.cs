using Npgsql;
using System.Data;
using System.Threading.Tasks;

namespace PostgreSQLStorage
{
    public class Database
    {
        private readonly string _connectionString;

        public Database(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Renamed to better reflect its purpose: getting a ready-to-use
        // NpgsqlConnection object that must be disposed of after use.
        public NpgsqlConnection GetConnection()
        {
            // Simply create and return a new connection. 
            // Npgsql will lazily open the connection when needed,
            // or when conn.OpenAsync() is explicitly called later.
            var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            return conn;
        }
    }
}
