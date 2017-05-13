using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebTorrent.Model;

namespace WebTorrent.Repository
{
    public interface IContentRecordRepository
    {
        IQueryable<Content> GetAll();
        Task<IList<Content>> Find(string folder);
        Task<Content> FindByHash(string hash);
        void Add(Content contentRecord);
        Task Delete(int id);
        void Save();
    }
}