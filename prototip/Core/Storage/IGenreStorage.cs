using Core.Model;

namespace Core.Storage
{
    public interface IGenreStorage
    {
        public Task<List<Genre>> GetEditorsGenres(string editorId);
    }
}
