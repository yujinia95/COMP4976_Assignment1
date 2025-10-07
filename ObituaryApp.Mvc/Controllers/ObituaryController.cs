using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObituaryApp.Mvc.Data;
using ObituaryApp.Mvc.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization; 

namespace ObituaryApp.Mvc.Controllers;

/*
    ObituaryController handles CRUD operations for obituaries.
    All endpoints return JSON responses.
*/
[ApiController]
[Route("api/[controller]")]
public class ObituaryController : ControllerBase
{
    private readonly ObituaryDbContext _dbContext;
    // For accessing to user/role info
    private readonly UserManager<IdentityUser> _userManager;
    // Fixed number of obituaries per page
    private const int PageSize = 10;

    // Constructor
    public ObituaryController(ObituaryDbContext dbContext, UserManager<IdentityUser> userManager)
    {
        _dbContext   = dbContext;
        _userManager = userManager;
    }


    /*
        Get all obituaries with pagination and optional search.
        Only show 10 obituaries per page.
    */
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAllObituaries(int page = 1)
    {
        const int PageSize = 10;
        if (page < 1) page = 1;

        var query = _dbContext.Obituaries.AsNoTracking();  // Just reading not editing
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(o => o.Id) // new obituaries first
            .Skip((page - 1) * PageSize)  // skip the obituaries from previous pages
            .Take(PageSize)               // grab only 10 obituaries
            .ToListAsync();

        return Ok(new {items, page, pageSize = PageSize, total = totalCount});
    }


    /*
        Reading one obituary by id
        Return 404 if not found
    */
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetObituaryById(int id)
    {
        var obituary = await _dbContext.Obituaries
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);
        return obituary is null ? NotFound() : Ok(obituary);
    }


    /*
        Creating a new obituary
        Return 400 if FullName is missing
        Return 201 with location header if created
    */
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateObituary([FromBody] Obituary requestBody)
    {   
        // If FullName is missing, return 400
        if (string.IsNullOrWhiteSpace(requestBody.FullName))
        {
            return BadRequest(new { message = "FullName is required." });
        }

        // Reading current user's ID from the JWT token
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        var obituaryEntity = new Obituary
        {
            FullName         = requestBody.FullName.Trim(),
            DateOfBirth      = requestBody.DateOfBirth,
            DateOfDeath      = requestBody.DateOfDeath,
            Biography        = requestBody.Biography,
            Photo            = requestBody.Photo,
            PhotoContentType = requestBody.PhotoContentType,
            CreatedByUserId  = userId
        };

        _dbContext.Obituaries.Add(obituaryEntity);
        await _dbContext.SaveChangesAsync();

        // This line of code returns a created response(201)
        return CreatedAtAction(nameof(GetObituaryById), new { id = obituaryEntity.Id }, obituaryEntity);
    }


    /*
        Editing an existing obituary
        Return 404 if not found
        Return 204 if updated
    */
    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Obituary requestBody)
    {
        var obituary = await _dbContext.Obituaries.FindAsync(id);

        // If obituary not found, return 404
        if (obituary is null)
        {
            return NotFound(new { message = "Obituary not found. :/" });
        }

        var userId  = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isOwner = obituary.CreatedByUserId == userId;
        var isAdmin = User.IsInRole("Admin");

        // Only the owner or an admin can edit
        if (!isOwner && !isAdmin)
        {
            return Forbid();
        }

        // Validate FullName. Cannot be empty.
        if (string.IsNullOrWhiteSpace(requestBody.FullName))
        {
            return BadRequest(new { message = "FullName is required." });
        }

        obituary.FullName = requestBody.FullName.Trim();
        obituary.DateOfBirth = requestBody.DateOfBirth;
        obituary.DateOfDeath = requestBody.DateOfDeath;
        obituary.Biography = requestBody.Biography;
        obituary.Photo = requestBody.Photo;
        obituary.PhotoContentType = requestBody.PhotoContentType;

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }


    /*
        Deleting an obituary
        Return 404 if not found
        Return 204 if deleted
    */
    [Authorize]
    [HttpDelete("{id:int}")]    
    public async Task<IActionResult> Delete(int id)
    {
        var obituary = await _dbContext.Obituaries.FindAsync(id);
        if (obituary is null)
        {
            return NotFound(new { message = "Obituary not found. :/" });
        }

        var userId  = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isOwner = obituary.CreatedByUserId == userId;
        var isAdmin = User.IsInRole("Admin");

        if (!isOwner && !isAdmin)
        {
            return Forbid();
        }
        
        _dbContext.Obituaries.Remove(obituary);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

}
