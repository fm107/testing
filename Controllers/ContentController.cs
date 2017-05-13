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
        public async Task<IActionResult> GetFolder([FromQuery] string folder)
        {
#if !DEBUG
            var directoryInfo = new DirectoryInfo(Path.Combine(_environment.WebRootPath, folder ?? UploadFolder));

            if (!directoryInfo.Parent.FullName.StartsWith(_environment.WebRootPath))
                return Forbid();
#endif
            var resp = await _fsInfo.GetFolderContent(folder);
            return Json(resp);
        }

        [HttpGet("[action]")]
        public string ShowDirectory()
        {
            string ret = null;
            foreach (var file in Directory.EnumerateFiles(Path.Combine(_environment.WebRootPath, UploadFolder), "*",
                SearchOption.AllDirectories))
                ret += string.Format("{0} Size: {1}", file, new FileInfo(file).Length) + Environment.NewLine;
            return ret;
        }

        [HttpGet("[action]")]
        public IActionResult RunUTorrent()
        {
            var processInfo = new ProcessStartInfo("/app/utorrent-server/utserver")
            {
                Arguments =
                    "-configfile /app/utorrent-server/utserver.conf -logfile /app/heroku_output/wwwroot/uploads/log.txt -daemon",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            };

            Process.Start(processInfo).StandardOutput.ReadToEndAsync()
                .ContinueWith(response => { _log.LogInformation(response.Result); });

            return Ok();
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> ShowProcess()
        {
            var processInfo = new ProcessStartInfo("ps")
            {
                Arguments = "-ef",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            };

            var process = Process.Start(processInfo);

            return Ok(await process.StandardOutput.ReadToEndAsync());
        }

        // GET: api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new[] {"value1", "value2"};
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }

    internal class MyClass
    {
        public string message { get; set; }
    }
}