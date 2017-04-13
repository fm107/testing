using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using WebTorrent.Model;
using System;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebTorrent.Controllers
{
    [Route("api/[controller]")]
    public class ContentController : Controller
    {
        private readonly IHostingEnvironment _environment;

        public ContentController(IHostingEnvironment environment)
        {
            _environment = environment;

            if (!Directory.Exists(Path.Combine(_environment.WebRootPath, "uploads")))
                Directory.CreateDirectory(Path.Combine(_environment.WebRootPath, "uploads"));
        }

        [HttpGet("[action]")]
        public IActionResult ShowFileSystem([FromQuery] string folder)
        {
            var directoryInfo = new DirectoryInfo(Path.Combine(_environment.WebRootPath, folder ?? "uploads"));

            if (!directoryInfo.Parent.FullName.StartsWith(_environment.WebRootPath))
                return Forbid();

            var listComponents = new List<FileSystem>(directoryInfo.GetFiles("*", SearchOption.TopDirectoryOnly)
                .Select(file => new FileSystem
                {
                    FullName = file.FullName.Replace(_environment.WebRootPath, string.Empty).TrimStart('\u005C'),
                    Name = file.Name,
                    Size = file.Length,
                    LastChanged = file.LastWriteTime,
                    Type = "file"
                }));

            listComponents.AddRange(directoryInfo.GetDirectories("*", SearchOption.TopDirectoryOnly)
                .Select(directory => new FileSystem
                    {
                        FullName = directory.FullName.Replace(_environment.WebRootPath, string.Empty).TrimStart('\u005C'),
                        Name = directory.Name,
                        Size = new DirectoryInfo(directory.FullName).GetFiles("*", SearchOption.AllDirectories)
                            .Sum(f => f.Length),
                        LastChanged = directory.LastWriteTime,
                        Type = "folder"
                    }
                ));

            var content = new Content
            {
                Contents = listComponents,
                Parent = folder == null || folder == "uploads"
                    ? null
                    : directoryInfo.Parent.FullName.Replace(_environment.WebRootPath, string.Empty).TrimStart('\u005C')
            };

            return Json(content);
        }
        
        [HttpGet("[action]")]
        public string ShowDirectory()
        {
            string ret = null;
            foreach (string file in Directory.EnumerateFiles(Path.Combine(_environment.WebRootPath, "uploads"), "*",SearchOption.AllDirectories))
            {
                ret += string.Format("{0} Size: {1}",file, new FileInfo(file).Length) + Environment.NewLine;

            }
            return ret;
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
}
