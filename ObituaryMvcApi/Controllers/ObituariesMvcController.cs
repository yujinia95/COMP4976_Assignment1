using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObivtuaryMvcApi.Data;
using ObivtuaryMvcApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ObivtuaryMvcApi.Controllers;

[Authorize] // Any authenticated user can access these actions (Admins still have elevated rights)
public class ObituariesMvcController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ObituariesMvcController> _logger;

    public ObituariesMvcController(ApplicationDbContext context, ILogger<ObituariesMvcController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: ObituariesMvc - Redirect to Home page since that's where obituaries are displayed
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Home");
    }

    // GET: ObituariesMvc/Details/5
    [AllowAnonymous] // Allow public access to view obituary details
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var obituary = await _context.Obituaries
            .FirstOrDefaultAsync(m => m.Id == id);
        if (obituary == null)
        {
            return NotFound();
        }

        return View(obituary);
    }

    // GET: ObituariesMvc/Create
    [AllowAnonymous]
    public IActionResult Create()
    {
        return View();
    }

    // POST: ObituariesMvc/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Create(ObituaryCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Get the current user's ID
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // If a user isn't authenticated, allow creation and store empty string
            // for CreatedByUserId so we don't require a DB migration.
            if (string.IsNullOrEmpty(userId))
            {
                userId = string.Empty;
            }

            var obituary = new Obituary
            {
                FullName = model.FullName,
                DateOfBirth = model.DateOfBirth,
                DateOfDeath = model.DateOfDeath,
                Biography = model.Biography,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Handle photo upload if provided
            if (model.PhotoFile != null && model.PhotoFile.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await model.PhotoFile.CopyToAsync(memoryStream);
                    obituary.Photo = memoryStream.ToArray();
                }
            }

            _context.Add(obituary);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Obituary created successfully!";
            return RedirectToAction("Index", "Home");
        }
        return View(model);
    }

    // GET: ObituariesMvc/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var obituary = await _context.Obituaries.FindAsync(id);
        if (obituary == null)
        {
            return NotFound();
        }

        // Check if the current user can edit this obituary
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");

        // If CreatedByUserId is null/empty, only Admins may modify.
        if ((string.IsNullOrEmpty(obituary.CreatedByUserId) || obituary.CreatedByUserId != userId) && !isAdmin)
        {
            TempData["Error"] = "You can only edit obituaries you created, or you must be an Admin.";
            return RedirectToAction("Index", "Home");
        }

        var viewModel = new ObituaryEditViewModel
        {
            Id = obituary.Id,
            FullName = obituary.FullName,
            DateOfBirth = obituary.DateOfBirth,
            DateOfDeath = obituary.DateOfDeath,
            Biography = obituary.Biography,
            HasExistingPhoto = obituary.Photo != null
        };

        return View(viewModel);
    }

    // POST: ObituariesMvc/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ObituaryEditViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var obituary = await _context.Obituaries.FindAsync(id);
                if (obituary == null)
                {
                    return NotFound();
                }

                // Check if the current user can edit this obituary
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var isAdmin = User.IsInRole("Admin");

                // If CreatedByUserId is null/empty, only Admins may modify.
                if ((string.IsNullOrEmpty(obituary.CreatedByUserId) || obituary.CreatedByUserId != userId) && !isAdmin)
                {
                    TempData["Error"] = "You can only edit obituaries you created, or you must be an Admin.";
                    return RedirectToAction("Index", "Home");
                }

                // Update properties
                obituary.FullName = model.FullName;
                obituary.DateOfBirth = model.DateOfBirth;
                obituary.DateOfDeath = model.DateOfDeath;
                obituary.Biography = model.Biography;
                obituary.UpdatedAt = DateTime.UtcNow;

                // Handle photo update / removal
                if (model.PhotoFile != null && model.PhotoFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.PhotoFile.CopyToAsync(memoryStream);
                        obituary.Photo = memoryStream.ToArray();
                    }
                }
                else if (model.RemovePhoto)
                {
                    // Clear the stored photo if user requested removal and didn't upload a new one
                    obituary.Photo = null;
                }

                _context.Update(obituary);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Obituary updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ObituaryExists(model.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction("Index", "Home");
        }
        return View(model);
    }

    // GET: ObituariesMvc/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var obituary = await _context.Obituaries
            .FirstOrDefaultAsync(m => m.Id == id);
        if (obituary == null)
        {
            return NotFound();
        }

        // Check if the current user can delete this obituary
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");
    var canDelete = (!string.IsNullOrEmpty(obituary.CreatedByUserId) && obituary.CreatedByUserId == userId) || isAdmin;

        var viewModel = new ObituaryDeleteViewModel
        {
            Obituary = obituary,
            CanDelete = canDelete
        };

        return View(viewModel);
    }

    // POST: ObituariesMvc/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var obituary = await _context.Obituaries.FindAsync(id);
        if (obituary == null)
        {
            return NotFound();
        }

        // Check if the current user can delete this obituary
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");

        // If CreatedByUserId is null/empty, only Admins may delete.
        if ((string.IsNullOrEmpty(obituary.CreatedByUserId) || obituary.CreatedByUserId != userId) && !isAdmin)
        {
            TempData["Error"] = "You can only delete obituaries you created, or you must be an Admin.";
            return RedirectToAction("Index", "Home");
        }

        _context.Obituaries.Remove(obituary);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Obituary deleted successfully!";
        return RedirectToAction("Index", "Home");
    }

    private bool ObituaryExists(int id)
    {
        return _context.Obituaries.Any(e => e.Id == id);
    }
}