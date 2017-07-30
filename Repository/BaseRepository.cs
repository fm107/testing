using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WebTorrent.Model;

namespace WebTorrent.Repository
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public abstract class BaseRepository<C, T> : IRepository<T> where T : class, IEntity where C : DbContext
    {
        private readonly C _context;

        protected BaseRepository(C context)
        {
            _context = context;
        }

        public virtual IQueryable<T> GetAll()
        {
            IQueryable<T> query = _context.Set<T>();
            return query;
        }

        public virtual void Add(T entity)
        {
            _context.Set<T>().Add(entity);
        }

        public virtual void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        public virtual void Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }

        public virtual IQueryable<T> FindBy(Expression<Func<T, bool>> predicate)
        {
            var query = _context.Set<T>().Where(predicate);
            return query;
        }

        public virtual void Edit(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }

        public virtual void Save()
        {
            _context.SaveChanges();
        }
    }
}