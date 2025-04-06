using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CADProjectsHub.Data;
using CADProjectsHub.Models;
using CADProjectsHub;
using CADProjectsHub.Crypto;
using Microsoft.AspNetCore.Authorization;
using CADProjectsHub.Helpers;
using Microsoft.Extensions.Options;

namespace CADProjectsHub.Controllers
{
    [Authorize]
    public class CADModelsController : Controller
    {
        private readonly CADProjectsContext _context;
        private readonly IConfiguration _configuration;
        private readonly IOptions<CryptoSettings> _cryptoSettings;
        public CADModelsController(CADProjectsContext context, IConfiguration configuration, IOptions<CryptoSettings> cryptoSettings)
        {
            _context = context;
            _configuration = configuration;
            _cryptoSettings = cryptoSettings;
        }

        // GET: CADModels
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var cADModels = from s in _context.CADModels
                           select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                cADModels = cADModels.Where(s => s.Name.Contains(searchString)
                                       || s.FileType.Contains(searchString));
            }

            var encryptionKey = _configuration["EncryptionSettings:AESKey"];

            foreach (var cad in cADModels)
            {
                if (cad != null &&
                    !string.IsNullOrEmpty(cad.ConstructorName) &&
                    !string.IsNullOrEmpty(cad.ConstructorInitializationVector))
                {
                    var dataProtection = new DataProtection(_cryptoSettings);
                    cad.ConstructorNameEncrypted = dataProtection.Decrypt(
                        cad.ConstructorName, encryptionKey, cad.ConstructorInitializationVector);
                }
                else
                {
                    cad.ConstructorNameEncrypted = cad.ConstructorName;
                }
            }

            switch (sortOrder)
            {
                case "name_desc":
                    cADModels = cADModels.OrderByDescending(s => s.Name);
                    break;
                case "Date":
                    cADModels = cADModels.OrderBy(s => s.AssignmentDate);
                    break;
                case "date_desc":
                    cADModels = cADModels.OrderByDescending(s => s.AssignmentDate);
                    break;
                default:
                    cADModels = cADModels.OrderBy(s => s.Name);
                    break;
            }

            var models = _context.CADModels
                         .Include(m => m.CADFiles) // Dodano Include, aby pobrać pliki!
                         .AsNoTracking();

            int pageSize = 10;
            return View(await PaginatedList<CADModel>.CreateAsync(models, pageNumber ?? 1, pageSize));
        }

        // GET: CADModels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cADModel = await _context.CADModels
                .Include(s => s.Assignments)
                    .ThenInclude(a => a.Project)
                .Include(m => m.CADFiles)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (cADModel == null)
            {
                return NotFound();
            }

            var encryptionKey = _configuration["EncryptionSettings:AESKey"];
            if (!string.IsNullOrEmpty(cADModel.ConstructorName) && !string.IsNullOrEmpty(cADModel.ConstructorInitializationVector))
            {
                var dataProtection = new DataProtection(_cryptoSettings);
                cADModel.ConstructorNameEncrypted = dataProtection.Decrypt(cADModel.ConstructorName, encryptionKey, cADModel.ConstructorInitializationVector);
            }


            return View(cADModel);
        }

        // GET: CADModels/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CADModels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,FileType,Manufacturing,ConstructorName,AssignmentDate")] CADModel cADModel)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    var encryptionKey = _configuration["EncryptionSettings:AESKey"];
                    if (!string.IsNullOrEmpty(cADModel.ConstructorName)) 
                    {
                        string IV;
                        var dataProtection = new DataProtection(_cryptoSettings);
                        cADModel.ConstructorName = dataProtection.Encrypt(cADModel.ConstructorName, encryptionKey, out IV);
                        cADModel.ConstructorInitializationVector = IV;
                    }

                    _context.Add(cADModel);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists, " +
                    "contact your system administrator.");
            }
            return View(cADModel);
        }

        // GET: CADModels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cADModelToUpdate = await _context.CADModels.FindAsync(id);
            if (cADModelToUpdate == null)
            {
                return NotFound();
            }
            return View(cADModelToUpdate);
        }
        // POST: CADModels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,FileType,Manufacturing,ConstructorName,AssignmentDate")] CADModel cADModel)
        {
            if (id != cADModel.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {

                    var encryptionKey = _configuration["EncryptionSettings:AESKey"];
                    if (!string.IsNullOrEmpty(cADModel.ConstructorName)) // Sprawdzenie, czy nie jest null
                    {
                        string IV;
                        var dataProtection = new DataProtection(_cryptoSettings);
                        cADModel.ConstructorName = dataProtection.Encrypt(cADModel.ConstructorName, encryptionKey, out IV);
                        cADModel.ConstructorInitializationVector = IV;
                    }

                    _context.Update(cADModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CADModelExists(cADModel.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(cADModel);
        }

        // GET: CADModels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cADModel = await _context.CADModels
                .FirstOrDefaultAsync(m => m.ID == id);
            if (cADModel == null)
            {
                return NotFound();
            }

            return View(cADModel);
        }

        // POST: CADModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cADModel = await _context.CADModels.FindAsync(id);
            if (cADModel != null)
            {
                _context.CADModels.Remove(cADModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CADModelExists(int id)
        {
            return _context.CADModels.Any(e => e.ID == id);
        }
    }
}
