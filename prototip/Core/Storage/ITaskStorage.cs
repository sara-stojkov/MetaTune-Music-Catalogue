using Core.Model;
using Task = System.Threading.Tasks.Task;

namespace Core.Storage
{
    public interface ITaskStorage
    {
        Task<Core.Model.Task?> GetById(string id);
        Task<List<Core.Model.Task>> GetAll();
        Task CreateOne(Core.Model.Task task);
        Task UpdateOne(Core.Model.Task task);
        Task DeleteById(string id);
        Task<List<Core.Model.Task>> GetAllByEditorId(string userId);
    }
}