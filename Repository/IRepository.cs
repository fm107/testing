using System.Linq;
using WebTorrent.Model;

namespace WebTorrent.Repository
{
    public interface IRepository<T> where T : class, IEntity
    {
        IQueryable<T> GetAll();
        void Add(T entity);
        void Delete(T entity);
        void Update(T entity);
    }
}