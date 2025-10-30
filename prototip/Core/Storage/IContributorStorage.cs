using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Core.Model;
using Task = System.Threading.Tasks.Task;

namespace Core.Storage
{
    public interface IContributorStorage
    {
        public Task<List<Contributor>> GetAll();
        public Task<List<Contributor>> GetAllByWorkId(string workId);
        public Task CreateOne(Contributor contributor);
        public Task UpdateOne(Contributor contributor);
        public Task DeleteById(string personId, string workId);
    }
}