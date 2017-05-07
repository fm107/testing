using System.Collections.Generic;
using WebTorrent.Model;

namespace WebTorrent.Repository
{
    public interface IContentRecordRepository
    {
        IList<Content> GetAll();
        IList<Content> Find(string folder);
        void Add(Content contentRecord);
        void Delete(int id);
        void Save();
    }
}