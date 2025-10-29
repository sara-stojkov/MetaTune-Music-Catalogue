using Core.Model;
using Task = System.Threading.Tasks.Task;

namespace Core.Storage
{
    public interface IUserStorage
    {
        public Task<User?> GetByEmail(string email);
        public Task<User?> GetById(string id);
        public Task<List<User>> GetAll();
        public Task<List<User>> GetAllByRole(string role);
        public Task CreateOne(User user);
        public Task UpdateOne(User user);
        public Task DeleteById(string id);
    }
}