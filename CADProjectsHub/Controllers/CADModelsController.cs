using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CADProjectsHub.Data;
using CADProjectsHub.Models;

namespace CADProjectsHub.Controllers
{
    public class CADModelsController : Controller
    {
        private readonly CADProjectsContext _context;

        public CADModelsController(CADProjectsContext context)
        {
            _context = context;
        }

        // GET: CADModels
        public async Task<IActionResult> Index()
        {
            return View(await _context.CADModels.ToListAsync());
        }

        // GET: CADModels/Details/5
        public async Task<IActionResult> Details(int? id)
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
        public async Task<IActionResult> Create([Bind("ID,Name,FileType,Manufacturing,ConstructorName,AssignmentDate")] CADModel cADModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cADModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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

            var cADModel = await _context.CADModels.FindAsync(id);
            if (cADModel == null)
            {
                return NotFound();
            }
            return View(cADModel);
        }

        // POST: CADModels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
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
