using Core.Model;
using Task = System.Threading.Tasks.Task;

namespace Core.Storage
{
    public interface IAuthorStorage
    {
        public Task<Author?> GetById(string id);
        public Task<List<Author>> GetAll();
        public Task CreateOne(Author author);
        public Task UpdateOne(Author author);
        public Task DeleteById(string id);
    }
}