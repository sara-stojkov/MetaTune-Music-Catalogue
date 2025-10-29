using Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Storage
{
    public interface IAuthorStorage
    {
        Task<Author?> GetById(string authorId);
        Task<List<Author>> GetAll();
        Task<List<Genre>> GetAuthorGenres(string authorId);
        Task<List<Work>> GetAuthorWorks(string authorId);
        Task<List<Member>> GetAuthorMemberships(string authorId);
        Task<decimal?> GetAverageRating(string authorId);
        Task<Rating?> GetUserRating(string authorId, string userId);
        Task<Review?> GetEditorReview(string authorId);
        Task<List<Review>> GetUserReviews(string authorId);
        System.Threading.Tasks.Task Add(Author author);
        System.Threading.Tasks.Task Update(Author author);
    }
}
