using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebTorrent.Data;
using WebTorrent.Model;

namespace WebTorrent.Repository
{
    internal class ContentRepository : BaseRepository<ContentDbContext, Content>, IContentRepository
    {
        private readonly ContentDbContext _context;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public ContentRepository(ContentDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IList<Content>> FindByFolder(string folder, bool needFiles, string hash)
        {
            await _semaphore.WaitAsync();

            try
            {
                if (needFiles)
                {
                    var contentbyHash = await FindByHash(hash, false, "FsItems");
                    contentbyHash.FsItems = contentbyHash.FsItems.Where(b => b.Type.Equals("file")).ToList();
                    return new[] {contentbyHash};
                }

                var contents = await _context.Content.Where(t => t.ParentFolder.Equals(folder)).Include(f => f.FsItems)
                    .AsNoTracking().ToListAsync();

                foreach (var content in contents)
                {
                    content.FsItems = content.FsItems.Where(b => b.Type.Equals("folder")).ToList();
                }

                return contents;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public async Task<Content> FindByHash(string hash, bool tracking, string include = null)
        {
            await _semaphore.WaitAsync();

            try
            {
                if (!string.IsNullOrEmpty(include) && tracking)
                {
                    return await _context.Content.Include(include).FirstOrDefaultAsync(t => t.Hash.Equals(hash));
                }

                if (!string.IsNullOrEmpty(include) && !tracking)
                {
                    return await _context.Content.Include(include).AsNoTracking()
                        .FirstOrDefaultAsync(t => t.Hash.Equals(hash));
                }

                if (string.IsNullOrEmpty(include) && tracking)
                {
                    return await _context.Content.FirstOrDefaultAsync(t => t.Hash.Equals(hash));
                }

                if (string.IsNullOrEmpty(include) && !tracking)
                {
                    return await _context.Content.AsNoTracking().FirstOrDefaultAsync(t => t.Hash.Equals(hash));
                }

                return await Task.FromResult<Content>(null);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Delete(params Content[] contentRecord)
        {
            _context.Content.RemoveRange(contentRecord);
        }

        public void Delete(params FileSystemItem[] fsItemsRecord)
        {
            _context.FsItem.RemoveRange(fsItemsRecord);
        }

        async Task IContentRepository.Save()
        {
            await _context.SaveChangesAsync();
        }
    }
}