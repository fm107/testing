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

        public IList<Content> GetAll()
        {
            _log.Info("Getting the existing records");
            return _context.Content.ToList();
        }

        public IList<Content> Find(string folder)
        {
            var k= _context.Content.Where(t => t.CurrentFolder.StartsWith(folder)).Include(t => t.FsItems).ToList();
            return k;
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