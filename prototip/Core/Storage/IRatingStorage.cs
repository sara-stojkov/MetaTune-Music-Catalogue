using Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Storage
{
    public interface IRatingStorage
    {
        Task<Rating?> GetById(string ratingId);
        Task<List<Rating>> GetByAuthorId(string authorId);
        Task<List<Rating>> GetByWorkId(string workId);
        Task<Rating?> GetUserRatingForAuthor(string userId, string authorId);
        Task<Rating?> GetUserRatingForWork(string userId, string workId);
        Task<decimal?> GetAverageRatingForAuthor(string authorId);
        Task<decimal?> GetAverageRatingForWork(string workId);
        Task<Rating?> GetEditorRatingForAuthor(string authorId);
        Task<Rating?> GetEditorRatingForWork(string workId);
        System.Threading.Tasks.Task Add(Rating rating);
        System.Threading.Tasks.Task Update(Rating rating);
        System.Threading.Tasks.Task Delete(string ratingId);
    }
}
