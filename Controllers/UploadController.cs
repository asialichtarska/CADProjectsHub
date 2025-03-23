using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CADProjectsHub.Data;
using CADProjectsHub.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CADProjectsHub.Controllers
{
    public class UploadController : Controller
    {
        private readonly CADProjectsContext _context;
        private readonly IWebHostEnvironment _environment;

        public UploadController(CADProjectsContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public IActionResult Index()
        {
            var model = new UploadViewModel
            {
                CADModels = _context.CADModels
                     .Include(m => m.CADFiles) // Pobranie powiązanych plików
                     .ToList()
            };

            return View("Upload",model);
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(UploadViewModel model)
        {
            if (model.File == null || model.File.Length == 0)
            {
                TempData["Error"] = "Please select a file.";
                return RedirectToAction("Index", "CADModels");
            }

            var allowedExtensions = new[] { ".step", ".stl", ".pdf" };
            var extension = Path.GetExtension(model.File.FileName).ToLower();

            if (!allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Invalid file type.";
                return RedirectToAction("Index", "CADModels");
            }



            // Znalezienie lub utworzenie modelu CAD
            CADModel selectedModel = null;

            if (model.SelectedCADModelID > 0)
            {
                selectedModel = _context.CADModels.Find(model.SelectedCADModelID);
                if (selectedModel == null)
                {
                    TempData["Error"] = "Selected CAD model not found.";
                    return RedirectToAction("Index");
                }
            }
            else if (!string.IsNullOrEmpty(model.NewCADModelName))
            {
                selectedModel = new CADModel
                {
                    Name = model.NewCADModelName,
                    FileType = extension,
                    Manufacturing = model.Manufacturing ?? "Unknown",
                    ConstructorName = model.ConstructorName ?? "Unknown",
                    AssignmentDate = DateTime.UtcNow,
                };

                _context.CADModels.Add(selectedModel);
                await _context.SaveChangesAsync();
            }
            else
            {
                TempData["Error"] = "Please select or enter a CAD model.";
                return RedirectToAction("Index");
            }


            // Zapis pliku na serwerze
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);
            var filePath = Path.Combine(uploadsFolder, model.File.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.File.CopyToAsync(stream);
            }

            // Dodanie pliku do bazy danych
            var newFile = new CADFile
            {
                FileName = model.File.FileName,
                FileType = extension,
                FilePath = $"/uploads/{model.File.FileName}",
                CADModelID = selectedModel.ID // Powiązanie z modelem
            };

            _context.CADFiles.Add(newFile);
            await _context.SaveChangesAsync();

            TempData["Success"] = "File uploaded successfully!";
            return RedirectToAction("Index", "CADModels");
        }

        /// Funkcja pobierania plików 
        public IActionResult DownloadFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return NotFound();
            }

            var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }

            var fileBytes = System.IO.File.ReadAllBytes(fullPath);
            var contentType = "application/octet-stream";
            var fileName = Path.GetFileName(fullPath);

            return File(fileBytes, contentType, fileName);
        }
    }
}