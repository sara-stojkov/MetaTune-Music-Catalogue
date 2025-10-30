using Core.Model;
using Task = System.Threading.Tasks.Task;

namespace Core.Storage
{
    public interface IReviewStorage
    {
        public Task<Review?> GetById(string id);
        public Task<List<Review>> GetAll();
        public Task<List<Review>> GetAllByUserId(string userId);
        public Task<List<Review>> GetAllByWorkId(string workId);
        public Task<List<Review>> GetAllByAuthorId(string authorId);
        public Task<List<Review>> GetAllPending();
        public Task<Review?> GetEditorReviewByWorkId(string workId);
        public Task CreateOne(Review review);
        public Task UpdateOne(Review review);
        public Task DeleteById(string id);
        public Task<List<Review>> GetAllApprovedByWorkId(string workId);
        public Task<List<Review>> GetAllApprovedByAuthorId(string authorId);
        public Task<Review?> GetEditorReviewByAuthorId(string authorId);
    }
}