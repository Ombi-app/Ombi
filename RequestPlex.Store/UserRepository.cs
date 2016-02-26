using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dapper.Contrib.Extensions;

namespace RequestPlex.Store
{
    public class UserRepository<T> : IRepository<T> where T : UserModel
    {
        public UserRepository(ISqliteConfiguration config)
        {
            Config = config;
        }

        private ISqliteConfiguration Config { get; set; }
        public long Insert(T entity)
        {
            using (var cnn = Config.DbConnection())
            {
                cnn.Open();
                return cnn.Insert(entity);
            }
        }

        public IEnumerable<T> GetAll()
        {
            using (var db = Config.DbConnection())
            {
                db.Open();
                var result = db.GetAll<T>();
                return result;
            }
        }

        public T Get(string id)
        {
            using (var db = Config.DbConnection())
            {
                db.Open();
                var result = db.GetAll<T>();
                var selected = result.FirstOrDefault(x => x.User == id);
                return selected;
            }
        }

        public T Get(int id)
        {
            throw new NotImplementedException();
        }

        public void Delete(T entity)
        {
            using (var db = Config.DbConnection())
            {
                db.Open();
                db.Delete(entity);
            }
        }

        public bool Update(T entity)
        {
            using (var db = Config.DbConnection())
            {
                db.Open();
                return db.Update(entity);
            }
        }
    }
}
