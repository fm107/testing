using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebTorrent.Data;
using WebTorrent.Model;

namespace WebTorrent.Repository
{
    internal class ContentRecordRepository : IContentRecordRepository, IDisposable
    {
        private readonly ContentDbContext _context;

        private readonly ILogger<ContentRecordRepository> _log;

        public ContentRecordRepository(ContentDbContext context, ILogger<ContentRecordRepository> log)
        {
            _context = context;
            _log = log;
        }

        public IQueryable<Content> GetAll()
        {
            return _context.Content;
        }

        public async Task<IList<Content>> Find(string folder)
        {
            return await _context.Content.Where(t => t.CurrentFolder.StartsWith(folder)).Include(t => t.FsItems)
                .ToListAsync();
        }

        public async Task<Content> FindByHash(string hash)
        {
            return await _context.Content.FirstAsync(t => t.Hash.Equals(hash));
        }

        public async void Add(Content contentRecord)
        {
            await _context.Content.AddAsync(contentRecord);
        }

        public async Task Delete(int id)
        {
            var entity = await _context.Content.FirstAsync(t => t.Id == id);
            _context.Content.Remove(entity);
        }

        public async void Save()
        {
            await _context.SaveChangesAsync();
        }

        public async void Dispose()
        {
            await _context.SaveChangesAsync();
            _context?.Dispose();
        }
    }
}