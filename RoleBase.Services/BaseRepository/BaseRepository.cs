using Microsoft.EntityFrameworkCore;
using RoleBase.DataBase.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoleBase.Services.BaseRepository
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        private readonly ApplicationDbContext db;
        protected DbSet<T> table;

        public BaseRepository(ApplicationDbContext db) 
        {
            this.db = db;
            table = db.Set<T>();
        }
        public T Delete(object id)
        {
            T existing = table.Find(id);
            table.Remove(existing);
            return existing;
        }

        public IEnumerable<T> GetAll()
        {
            return table.ToList();
        }

        public T GetById(object id)
        {
            return table.Find(id);
        }

        public void Insert(T obj)
        {
            table.Add(obj);
            Save();
        }

        public T Update(T obj)
        {
            table.Attach(obj);
            db.Entry(obj).State = EntityState.Modified;
            Save();
            return obj;
        }

        private void Save()
        {
            db.SaveChanges();
        }
    }
}
