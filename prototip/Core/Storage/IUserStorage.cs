using Core.Model;

namespace Core.Storage
{
    public interface IUserStorage
    {
        public Task<User?> GetByEmail(string email);
        public Task<User?> GetById(string id);
        public Task<List<Genre>> GetEditorsGenres(string editorId);
    }
}
