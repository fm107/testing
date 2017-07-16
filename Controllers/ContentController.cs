using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebTorrent.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebTorrent.Controllers
{
    [Route("api/[controller]")]
    public class ContentController : Controller
    {
        private const string UploadFolder = "uploads";
        private readonly IHostingEnvironment _environment;
        private readonly FsInfo _fsInfo;
        private readonly ILogger<ContentController> _log;

        public ContentController(ILogger<ContentController> log, IHostingEnvironment environment, FsInfo fsInfo)
        {
            _environment = environment;
            _fsInfo = fsInfo;
            _log = log;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetFolder([FromQuery] string folder, bool needFiles, string hash)
        {
#if !DEBUG
            var directoryInfo = new DirectoryInfo(Path.Combine(_environment.WebRootPath, folder ?? UploadFolder));

            if (!directoryInfo.Parent.FullName.StartsWith(_environment.WebRootPath))
                return Forbid();
#endif
            var resp = await _fsInfo.GetFolderContent(folder, needFiles, hash);
            return Json(resp);
        }
    }

    internal class MyClass
    {
        public string message { get; set; }
    }
}