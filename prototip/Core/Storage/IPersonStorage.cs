using Core.Model;
using Task = System.Threading.Tasks.Task;

namespace Core.Storage
{
    public interface IPersonStorage
    {
        Task<Person?> GetById(string id);
        Task<List<Person>> GetAll();
        Task CreateOne(Person person);
        Task UpdateOne(Person person);
        Task DeleteById(string id);
    }
}