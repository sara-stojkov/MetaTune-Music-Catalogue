using Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Storage
{
    public interface IReviewStorage
    {
        Task<Review?> GetById(string reviewId);
        Task<List<Review>> GetByAuthorId(string authorId);
        Task<List<Review>> GetByWorkId(string workId);
        Task<Review?> GetEditorReviewForAuthor(string authorId);
        Task<Review?> GetEditorReviewForWork(string workId);
        Task<List<Review>> GetUserReviewsForAuthor(string authorId);
        Task<List<Review>> GetUserReviewsForWork(string workId);
        Task<Review?> GetUserReview(string userId, string? authorId, string? workId);
        System.Threading.Tasks.Task Add(Review review);
        System.Threading.Tasks.Task Update(Review review);
        System.Threading.Tasks.Task Delete(string reviewId);
        System.Threading.Tasks.Task Approve(string reviewId, string editorId);
    }
}
