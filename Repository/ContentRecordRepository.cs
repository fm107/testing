using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Microsoft.EntityFrameworkCore;
using WebTorrent.Data;
using WebTorrent.Model;

namespace WebTorrent.Repository
{
    internal class ContentRecordRepository : IContentRecordRepository, IDisposable
    {
        private readonly ContentDbContext _context;

        private readonly ILog _log;

        public ContentRecordRepository(ContentDbContext context)
        {
            _context = context;
            _log = _log = LogManager.GetLogger(Assembly.GetEntryAssembly(), "ContentRecordRepository");
        }

        public IQueryable<Content> GetAll()
        {
            return _context.Content;
        }

        public IList<Content> Find(string folder)
        {
            return _context.Content.Where(t => t.CurrentFolder.StartsWith(folder)).Include(t => t.FsItems).ToList();
        }

        public Content FindByHash(string hash)
        {
            return _context.Content.First(t => t.Hash.Equals(hash));
        }

        public async void Add(Content contentRecord)
        {
            await _context.Content.AddAsync(contentRecord);
        }

        public void Delete(int id)
        {
            var entity = _context.Content.First(t => t.Id == id);
            _context.Content.Remove(entity);
        }

        public async void Save()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.SaveChanges();
            _context?.Dispose();
        }
    }
}