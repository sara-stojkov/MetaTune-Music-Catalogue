
using Core.Model;
using Task = System.Threading.Tasks.Task;

namespace Core.Storage
{
    public interface IWorkStorage
    {
        public Task<Work?> GetById(string id);
        public Task<List<Work>> GetAll();
        public Task<List<Work>> GetAllByGenreId(string genreId);
        public Task<List<Work>> GetAllByAuthorId(string authorId);
        public Task<List<Work>> GetAllByAlbumId(string albumId);
        public Task CreateOne(Work work);
        public Task UpdateOne(Work work);
        public Task DeleteById(string id);
        public Task DeleteAllByAlbumId(string albumId);
    }
}