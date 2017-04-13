using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Torrent.Client;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebTorrent.Controllers
{
    [Route("api/[controller]")]
    public class TorrentController : Controller
    {
        private readonly string _uploads;

        public TorrentController(IHostingEnvironment environment)
        {
            _uploads = Path.Combine(environment.WebRootPath, "uploads");

            if (!Directory.Exists(_uploads))
                Directory.CreateDirectory(_uploads);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UploadFile(ICollection<IFormFile> file)
        {
            string fileName = null;
            foreach (var uploadedFile in file)
                if (uploadedFile.Length > 0)
                {
                    fileName = Path.Combine(_uploads, uploadedFile.FileName.Split('\\').LastOrDefault());

                    using (var fileStream = new FileStream(fileName, FileMode.Create))
                    {
                        await uploadedFile.CopyToAsync(fileStream);
                    }
                }
            var torrent = new TorrentTransfer(fileName, _uploads);
            torrent.Start();

            return Accepted();
        }
    }
}