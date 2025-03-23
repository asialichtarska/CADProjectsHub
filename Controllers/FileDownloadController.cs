using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading.Tasks;

namespace CADProjectsHub.Controllers
{
    public class FileDownloadController : Controller
    {
        private readonly IWebHostEnvironment _environment;

        public FileDownloadController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("No name of file.");
            }

            var filePath = Path.Combine(_environment.WebRootPath, "uploads", fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File doesn't exist.");
            }

            var memory = new MemoryStream();
            await using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                await stream.CopyToAsync(memory);
            }

            memory.Position = 0;
            return File(memory, "application/octet-stream", fileName);
        }
    }
}
