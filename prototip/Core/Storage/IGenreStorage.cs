using Core.Model;
using Task = System.Threading.Tasks.Task;

namespace Core.Storage
{
    public interface IGenreStorage
    {
        Task<Genre?> GetById(string id);
        Task<List<Genre>> GetAll();
        Task CreateOne(Genre genre);
        Task UpdateOne(Genre genre);
        Task DeleteById(string id);
        Task<List<Genre>> GetEditorsGenres(string editorId);
    }
}