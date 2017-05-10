using System.Collections.Generic;
using System.Linq;
using WebTorrent.Model;

namespace WebTorrent.Repository
{
    public interface IContentRecordRepository
    {
        IQueryable<Content> GetAll();
        IList<Content> Find(string folder);
        Content FindByHash(string hash);
        void Add(Content contentRecord);
        void Delete(int id);
        void Save();
    }
}