using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dapper.Contrib.Extensions;

namespace RequestPlex.Store
{
    public class GenericRepository<T> : IRepository<T> where T : Entity
    {
        public GenericRepository(ISqliteConfiguration config)
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
            throw new NotImplementedException();
        }

        public T Get(int id)
        {
            using (var db = Config.DbConnection())
            {
                db.Open();
                return db.Get<T>(id);
            }
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
