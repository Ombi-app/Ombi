using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Ombi.Store.Repository
{
    public interface IExternalUserRepository<T> where T : Entity
    {
        T Get(string id);
        T Get(int id);
        Task<T> GetAsync(string id);
        Task<T> GetAsync(int id);
        T GetUser(string userGuid);
        Task<T> GetUserAsync(string userguid);
        T GetUserByUsername(string username);

        IEnumerable<T> Custom(Func<IDbConnection, IEnumerable<T>> func);
        long Insert(T entity);
        void Delete(T entity);
        IEnumerable<T> GetAll();
        bool UpdateAll(IEnumerable<T> entity);
        bool Update(T entity);
        Task<IEnumerable<T>> GetAllAsync();
        Task<bool> UpdateAsync(T users);
        Task<int> InsertAsync(T users);
    }
}