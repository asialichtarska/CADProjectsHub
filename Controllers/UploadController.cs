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
using CADProjectsHub.Helpers;

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

            return View("Upload", model);
        }

        [HttpGet]
        public IActionResult GenerateKeys()
        {
            var rsaHelper = new RSAHelper();
            var (publicKey, privateKey) = ("", "");

            using (var rsa = new System.Security.Cryptography.RSACryptoServiceProvider(2048))
            {
                publicKey = rsa.ToXmlString(false);
                privateKey = rsa.ToXmlString(true);
            }

            // Zapis kluczy w odpowiednich lokalizacjach
            var keysFolder = Path.Combine(_environment.WebRootPath, "keys");
            Directory.CreateDirectory(Path.Combine(_environment.WebRootPath, "keys"));
            System.IO.File.WriteAllText(Path.Combine(_environment.WebRootPath, "keys/publicKey.xml"), publicKey);
            System.IO.File.WriteAllText(Path.Combine(_environment.WebRootPath, "keys/privateKey.xml"), privateKey);

            TempData["Success"] = "RSA keys generated successfully!";
            return RedirectToAction("Index");
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

            // Szyfrowanie pliku przed zapisem
            var rsaHelper = new RSAHelper();
            var publicKey = System.IO.File.ReadAllText(Path.Combine(_environment.WebRootPath, "keys/publicKey.xml"));

            using var memoryStream = new MemoryStream();
            await model.File.CopyToAsync(memoryStream);
            var fileData = memoryStream.ToArray();

            // Szyfrowanie i podpisywanie
            var encryptedData = rsaHelper.EncryptFile(fileData, publicKey);

            // Zapis zaszyfrowanego pliku na serwerze
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var encryptedFileName = model.File.FileName + ".enc";
            var filePath = Path.Combine(uploadsFolder, encryptedFileName);

            await System.IO.File.WriteAllBytesAsync(filePath, encryptedData);


            // Dodanie pliku do bazy danych
            var newFile = new CADFile
            {
                FileName = encryptedFileName,
                FileType = extension,
                FilePath = $"/uploads/{encryptedFileName}",
                CADModelID = selectedModel.ID
            };

            _context.CADFiles.Add(newFile);
            await _context.SaveChangesAsync();

            TempData["Success"] = "File encrypted and uploaded successfully!";
            return RedirectToAction("Index", "CADModels");
        }

        // Metoda do deszyfrowania i pobierania pliku
        public IActionResult DownloadFile(int id)
        {
            var fileRecord = _context.CADFiles.Find(id);
            if (fileRecord == null || string.IsNullOrEmpty(fileRecord.FilePath))
            {
                return NotFound();
            }

            var fullPath = Path.Combine(_environment.WebRootPath, fileRecord.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }

            var rsaHelper = new RSAHelper();
            var privateKey = System.IO.File.ReadAllText(Path.Combine(_environment.WebRootPath, "keys", "privateKey.xml"));

            // Odczyt i deszyfrowanie danych
            var encryptedData = System.IO.File.ReadAllBytes(fullPath);
            var decryptedData = rsaHelper.DecryptFile(encryptedData, privateKey);

            var contentType = "application/octet-stream";
            var originalFileName = fileRecord.FileName.Replace(".enc", "");

            return File(decryptedData, contentType, originalFileName);
        }

    }
}