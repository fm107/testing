using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebTorrent.Model;

namespace WebTorrent.Repository
{
    public interface IContentRecordRepository
    {
        IQueryable<Content> GetAll();
        Task<IList<Content>> FindByFolder(string folder, bool needFiles, string hash);
        Task<Content> FindByHash(string hash, string include = null);
        void Add(Content contentRecord);
        void Update(Content contentRecord);
        Task Delete(int id);
        void Save();
    }
}