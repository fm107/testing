using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

        public async Task<IList<Content>> FindByFolder(string folder, bool needFiles, string hash)
        {
            if (needFiles)
            {
                var contentbyHash = await FindByHash(hash, false, "FsItems");
                contentbyHash.FsItems = contentbyHash.FsItems.Where(b => b.Type.Equals("file")).ToList();
                return new[] {contentbyHash};
            }

            //todo correct display parent/current folder. Was t => t.CurrentFolder.Equals(folder))
            var contents = await _context.Content.Where(t => t.ParentFolder.Equals(folder)).Include(f => f.FsItems)
                .AsNoTracking().ToListAsync();

            foreach (var content in contents)
            {
                content.FsItems = content.FsItems =
                        content.FsItems.Where(b => b.Type.Equals("folder")).ToList();
            }

            return contents;
        }

        public async Task<Content> FindByHash(string hash, bool tracking, string include = null)
        {
            return !string.IsNullOrEmpty(include) && tracking
                ? await _context.Content.Include(include).FirstOrDefaultAsync(t => t.Hash.Equals(hash))
                : (string.IsNullOrEmpty(include) || tracking
                    ? (string.IsNullOrEmpty(include) && tracking
                        ? await _context.Content.FirstOrDefaultAsync(t => t.Hash.Equals(hash))
                        : await _context.Content.Include(include).AsNoTracking()
                            .FirstOrDefaultAsync(t => t.Hash.Equals(hash)))
                    : await _context.Content.Include(include).AsNoTracking()
                        .FirstOrDefaultAsync(t => t.Hash.Equals(hash)));
        }

        public async void Add(Content contentRecord)
        {
            await _context.Content.AddAsync(contentRecord);
        }
        public void Update(Content contentRecord)
        {
           _context.Content.Update(contentRecord);
        }


        public async Task Delete(int id)
        {
            var entity = await _context.Content.FirstOrDefaultAsync(t => t.Id == id);
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