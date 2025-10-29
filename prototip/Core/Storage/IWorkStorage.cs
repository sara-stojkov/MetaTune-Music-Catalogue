using Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Storage
{
    public interface IWorkStorage
    {
        Task<Work?> GetById(string workId);
        Task<List<Work>> GetAll();
        Task<List<Work>> GetByAlbumId(string albumId);
        Task<List<Author>> GetWorkAuthors(string workId);
        Task<List<Contributor>> GetWorkContributors(string workId);
        Task<decimal?> GetAverageRating(string workId);
        Task<Rating?> GetUserRating(string workId, string userId);
        Task<Review?> GetEditorReview(string workId);
        Task<List<Review>> GetUserReviews(string workId);
        Task<Genre?> GetWorkGenre(string workId);
        System.Threading.Tasks.Task Add(Work work);
        System.Threading.Tasks.Task Update(Work work);
    }
}
