using Core.Model;

namespace Core.Storage
{
    public interface IUserStorage
    {
        public Task<User?> GetByEmail(string email);
        public Task<User?> GetById(string id);
        public Task<string?> GetVerificationCode(string userId);
        public System.Threading.Tasks.Task Create(User user);
        public System.Threading.Tasks.Task Update(User user);
    }
}
