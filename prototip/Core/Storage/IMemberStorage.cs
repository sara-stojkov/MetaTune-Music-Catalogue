using Core.Model;
using Task = System.Threading.Tasks.Task;

namespace Core.Storage
{
    public interface IMemberStorage
    {
        Task<Member?> GetById(string groupId, string memberId);
        Task<List<Member>> GetAll();
        Task CreateOne(Member member);
        Task UpdateOne(Member member);
        Task DeleteById(string groupId, string memberId);
        Task<List<Member>> GetAllMembersPresentByAuthorId(string authorId);
        Task<List<Member>> GetAllMembersAllTimeByAuthorId(string authorId);

        Task<List<Member>> GetAllMembersByMemberId(string memberID);
    }
}