using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using MimeMapping;
using Newtonsoft.Json;
using WebTorrent.Extensions;
using WebTorrent.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebTorrent.Controllers
{
    [Route("api/[controller]")]
    public class ContentController : Controller
    {
        private const string UploadFolder = "uploads";
        private readonly IHostingEnvironment _environment;
        private ILog _log;

        public ContentController(IHostingEnvironment environment)
        {
            _environment = environment;
            _log = LogManager.GetLogger(Assembly.GetEntryAssembly(), "ContentController");

            if (!Directory.Exists(Path.Combine(_environment.WebRootPath, UploadFolder)))
                Directory.CreateDirectory(Path.Combine(_environment.WebRootPath, UploadFolder));
        }

        [HttpGet("[action]")]
        public IActionResult GetFolder([FromQuery] string folder)
        {
            var directoryInfo = new DirectoryInfo(Path.Combine(_environment.WebRootPath, folder ?? UploadFolder));

            if (!directoryInfo.Parent.FullName.StartsWith(_environment.WebRootPath))
                return Forbid();

            var listComponents = new List<FileSystem>(directoryInfo
                .GetFilesByExtensions(SearchOption.TopDirectoryOnly, MimeTypes.TypeMap
                    .Where(t => t.Value.Contains("video") | t.Value.Contains("audio"))
                    .Select(f => "*." + f.Key)
                    .ToArray())
                .Select(file => new FileSystem
                {
                    FullName = file.FullName.Replace(_environment.WebRootPath, string.Empty)
                        .TrimStart('\u005C', '\u002F'),
                    Name = file.Name,
                    Size = file.Length,
                    LastChanged = file.LastWriteTime,
                    Type = "file"
                }));

            listComponents.AddRange(directoryInfo.GetDirectories("*", SearchOption.TopDirectoryOnly)
                .Select(directory => new FileSystem
                {
                    FullName = directory.FullName.Replace(_environment.WebRootPath, string.Empty)
                            .TrimStart('\u005C', '\u002F'),
                    Name = directory.Name,
                    Size = new DirectoryInfo(directory.FullName).GetFiles("*", SearchOption.AllDirectories)
                            .Sum(f => f.Length),
                    LastChanged = directory.LastWriteTime,
                    Type = "folder"
                }
                ));

            var content = new Content
            {
                CurrentFolder = folder ?? UploadFolder,
                Contents = listComponents,
                Parent = folder == null || folder == UploadFolder
                    ? null
                    : directoryInfo.Parent.FullName.Replace(_environment.WebRootPath, string.Empty)
                        .TrimStart('\u005C', '\u002F')
            };

            return Json(content);
        }

        [HttpGet("[action]")]
        public string ShowDirectory()
        {
            string ret = null;
            foreach (var file in Directory.EnumerateFiles("", "*",
                SearchOption.AllDirectories))
                ret += string.Format("{0} Size: {1}", file, new FileInfo(file).Length) + Environment.NewLine;
            return ret;
        }

        [HttpGet("[action]")]
        public IActionResult RunUTorrent()
        {
            var processInfo = new ProcessStartInfo("/app/utorrent-server/utserver")
            {
                Arguments = "-configfile /app/utorrent-server/utserver.conf -logfile /app/heroku_output/wwwroot/uploads/log.txt -daemon",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            };

            Process.Start(processInfo).StandardOutput.ReadToEndAsync()
                .ContinueWith(response =>
                {
                    _log.Info(response.Result);
                }); 

            return Ok();
        }

        [HttpGet("[action]")]
        public IActionResult ShowProcess()
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

            return Ok(process.StandardOutput.ReadToEnd());
        }

        // GET: api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new[] { "value1", "value2" };
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

    class MyClass
    {
        public string message { get; set; }
    }
}