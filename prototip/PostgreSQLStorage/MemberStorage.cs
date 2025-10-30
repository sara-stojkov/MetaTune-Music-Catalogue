using Core.Model;
using Core.Storage;
using Npgsql;
using Task = System.Threading.Tasks.Task;

namespace PostgreSQLStorage
{
    public class MemberStorage : IMemberStorage
    {
        private readonly Database _db;

        public MemberStorage(Database db)
        {
            _db = db;
        }

        public async Task<Member?> GetById(string groupId, string memberId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT joinDate, leaveDate, groupId, memberId 
                  FROM members 
                  WHERE groupId = @groupId AND memberId = @memberId", conn);

            cmd.Parameters.AddWithValue("groupId", groupId);
            cmd.Parameters.AddWithValue("memberId", memberId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapReaderToMember(reader);
            }

            return null;
        }

        public async Task<List<Member>> GetAll()
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT joinDate, leaveDate, groupId, memberId 
                  FROM members 
                  ORDER BY groupId, joinDate", conn);

            using var reader = await cmd.ExecuteReaderAsync();

            var members = new List<Member>();

            while (await reader.ReadAsync())
            {
                members.Add(MapReaderToMember(reader));
            }

            return members;
        }

        public async Task CreateOne(Member member)
        {
            using var conn = _db.GetConnection();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                string sql = @"INSERT INTO members(joinDate, leaveDate, groupId, memberId) 
                               VALUES(@joinDate, @leaveDate, @groupId, @memberId)";

                using var cmd = new NpgsqlCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("joinDate", member.JoinDate);
                cmd.Parameters.AddWithValue("leaveDate", (object?)member.LeaveDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("groupId", member.GroupId);
                cmd.Parameters.AddWithValue("memberId", member.MemberId);

                await cmd.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateOne(Member member)
        {
            using var conn = _db.GetConnection();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                string sql = @"UPDATE members 
                               SET joinDate = @joinDate, 
                                   leaveDate = @leaveDate 
                               WHERE groupId = @groupId AND memberId = @memberId";

                using var cmd = new NpgsqlCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("joinDate", member.JoinDate);
                cmd.Parameters.AddWithValue("leaveDate", (object?)member.LeaveDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("groupId", member.GroupId);
                cmd.Parameters.AddWithValue("memberId", member.MemberId);

                await cmd.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteById(string groupId, string memberId)
        {
            using var conn = _db.GetConnection();
            string sql = @"DELETE FROM members WHERE groupId = @groupId AND memberId = @memberId";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("groupId", groupId);
            cmd.Parameters.AddWithValue("memberId", memberId);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<Member>> GetAllMembersPresentByAuthorId(string authorId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT joinDate, leaveDate, groupId, memberId 
                  FROM members 
                  WHERE groupId = @authorId AND leaveDate IS NULL 
                  ORDER BY joinDate", conn);

            cmd.Parameters.AddWithValue("authorId", authorId);

            using var reader = await cmd.ExecuteReaderAsync();

            var members = new List<Member>();

            while (await reader.ReadAsync())
            {
                members.Add(MapReaderToMember(reader));
            }

            return members;
        }

        public async Task<List<Member>> GetAllMembersAllTimeByAuthorId(string authorId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT joinDate, leaveDate, groupId, memberId 
                  FROM members 
                  WHERE groupId = @authorId 
                  ORDER BY joinDate", conn);

            cmd.Parameters.AddWithValue("authorId", authorId);

            using var reader = await cmd.ExecuteReaderAsync();

            var members = new List<Member>();

            while (await reader.ReadAsync())
            {
                members.Add(MapReaderToMember(reader));
            }

            return members;
        }

        public async Task<List<Member>> GetAllMembersByMemberId(string memberId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT joinDate, leaveDate, groupId, memberId 
                  FROM members 
                  WHERE memberId = @memberId 
                  ORDER BY joinDate", conn);
            cmd.Parameters.AddWithValue("memberId", memberId);
            using var reader = await cmd.ExecuteReaderAsync();
            var members = new List<Member>();
            while (await reader.ReadAsync())
            {
                members.Add(MapReaderToMember(reader));
            }
            return members;
        }

        private Member MapReaderToMember(NpgsqlDataReader reader)
        {
            var joinDate = reader.GetDateTime(0);
            var leaveDate = reader.IsDBNull(1) ? null : (DateTime?)reader.GetDateTime(1);
            var groupId = reader.GetString(2);
            var memberId = reader.GetString(3);

            return new Member(joinDate, leaveDate, groupId, memberId);
        }
    }
}