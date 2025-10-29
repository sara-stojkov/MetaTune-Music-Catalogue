using Core.Model;
using Task = System.Threading.Tasks.Task;

namespace Core.Storage
{
    public interface IReviewStorage
    {
        Task<Review> GetById(string reviewId);
        Task<List<Review>> GetAll();
        Task Add(Review review);
        Task Update(Review review);
        Task Delete(string reviewId);
        Task<List<Review>> GetAllByUserId(string userId);
        Task<List<Review>> GetAllByWorkId(string workId);
        Task<List<Review>> GetAllByAuthorId(string artistId);
        Task<Review> GetEditorReviewByWorkId(string workId);
    }
}
