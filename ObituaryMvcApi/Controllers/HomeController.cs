using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObituaryMvcApi.Data;
using ObituaryMvcApi.Models;

namespace ObituaryMvcApi.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index(string searchTerm, int page = 1)
    {
        const int pageSize = 6; // Show 5 obituaries at a time
        
        // Get obituaries for public display on home page
        var obituariesQuery = _context.Obituaries.AsQueryable();
        
        // Apply search filter if search term is provided
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            obituariesQuery = obituariesQuery.Where(o => 
                o.FullName.ToLower().Contains(searchTerm.ToLower()));
        }
        
        // Get total count for pagination
        var totalCount = await obituariesQuery.CountAsync();
        
        // Apply pagination
        var obituaries = await obituariesQuery
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        // Calculate pagination info
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        
        // Pass data to view for maintaining state
        ViewBag.SearchTerm = searchTerm;
        ViewBag.SearchResultCount = totalCount; // Total results, not just current page
        ViewBag.IsSearchActive = !string.IsNullOrWhiteSpace(searchTerm);
        
        // Pagination info
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalCount = totalCount;
        ViewBag.HasPreviousPage = page > 1;
        ViewBag.HasNextPage = page < totalPages;
        ViewBag.ShowingFrom = totalCount > 0 ? ((page - 1) * pageSize) + 1 : 0;
        ViewBag.ShowingTo = Math.Min(page * pageSize, totalCount);
        
        return View(obituaries);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
