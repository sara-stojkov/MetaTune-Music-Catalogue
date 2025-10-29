using Core.Model;
using Task = System.Threading.Tasks.Task;

namespace Core.Storage
{
    public interface IRatingStorage
    {
        Task<Rating?> GetById(string ratingId);
        Task<List<Rating>> GetAll();
        Task CreateOne(Rating rating);
        Task UpdateOne(Rating rating);
        Task DeleteById(string ratingId);
        Task<List<Rating>> GetAllByUserId(string userId);
        Task<List<Rating>> GetAllByWorkId(string workId);
        Task<List<Rating>> GetAllByAuthorId(string artistId);
    }
}
