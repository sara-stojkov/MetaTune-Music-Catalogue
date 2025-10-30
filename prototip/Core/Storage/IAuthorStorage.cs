using Core.Model;
using Task = System.Threading.Tasks.Task;

namespace Core.Storage
{
    public interface IAuthorStorage
    {
        Task<Author?> GetById(string id);
        Task<List<Author>> GetAll(AuthorFilter filter);
        Task CreateOne(Author author, Person? person = null);
        Task UpdateOne(Author author);
        Task DeleteById(string id);
        Task<List<Author>> GetAllAuthorsByGenreId(string genreId);
    }

    public enum AuthorFilter
    {
        All,
        Group,
        Solo
    }
}