using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObituaryMvcApi.Data;
using ObituaryMvcApi.Models;
using System.Security.Claims;

namespace ObituaryMvcApi.Controllers;

/**
  API Controller for managing obituaries.
  Supports CRUD operations with appropriate authorization.
*/
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ApiPolicy")]
public class ObituariesApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ObituariesApiController> _logger;

    /**
    * Constructor to initialize the controller with database context and logger.
    */
    public ObituariesApiController(ApplicationDbContext context, ILogger<ObituariesApiController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /**
    * Helper method for date validation
    */
    private string? ValidateDates(DateTime dob, DateTime dod)
    {
        if (dod < dob)
        {
            return "Date of Death cannot be before Date of Birth.";
        }

        if (dob > DateTime.Now)
        {
            return "Date of Birth cannot be in the future.";
        }

        if (dod > DateTime.Now)
        {
            return "Date of Death cannot be in the future.";
        }

        return null;
    }

    // Get all obituaries.
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Obituary>>> GetObituaries()
    {
        var items = await _context.Obituaries.ToListAsync();
        return Ok(items);
    }


    // Get obituary by ID.
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<Obituary>> GetObituary(int id)
    {
        var obituary = await _context.Obituaries.FindAsync(id);

        // If not found, return 404
        if (obituary == null)
        {
            return NotFound($"Obituary with ID {id} not found.");
        }
        
        return Ok(obituary);
    }


    // Create a new obituary.
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Obituary>> CreateObituary([FromBody] Obituary inputObituary)
    {

        // Using helper method for date validation
        var validationError = ValidateDates(inputObituary.DateOfBirth, inputObituary.DateOfDeath);
        // If validation fails, return 400 Bad Request with error message
        if (validationError != null)
        {
            return BadRequest(validationError);
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        var obituary = new Obituary
        {
            FullName        = inputObituary.FullName,
            DateOfBirth     = inputObituary.DateOfBirth,
            DateOfDeath     = inputObituary.DateOfDeath,
            Biography       = inputObituary.Biography,
            Photo           = inputObituary.Photo,
            CreatedByUserId = userId,
            CreatedAt       = DateTime.UtcNow,
            UpdatedAt       = DateTime.UtcNow
        };

        _context.Obituaries.Add(obituary);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetObituary), new { id = obituary.Id }, obituary);
    }

    // Update an existing obituary.
    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateObituary(int id, [FromBody] Obituary inputObituary)
    {
        var obituary = await _context.Obituaries.FindAsync(id);
        // If obituary not found, return 404
        if (obituary == null)
        {
            return NotFound($"Obituary with ID {id} not found.");
        }
        
        // Check if the current user is the creator or an admin
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");

        // If there is no createdByUserId or doesn't match with current user and creator and is not admin will get forbidden.
        if ((string.IsNullOrEmpty(obituary.CreatedByUserId) || obituary.CreatedByUserId != userId) && !isAdmin)
        {
            return Forbid("You can only update obituaries you created, or you must be an Admin.");
        }

        // Using helper method for date validation
        var validationError = ValidateDates(inputObituary.DateOfBirth, inputObituary.DateOfDeath);
        // If validation fails, return 400 Bad Request with error message
        if (validationError != null)
        {
            return BadRequest(validationError);
        }

        obituary.FullName    = inputObituary.FullName;
        obituary.DateOfBirth = inputObituary.DateOfBirth;
        obituary.DateOfDeath = inputObituary.DateOfDeath;
        obituary.Biography   = inputObituary.Biography;
        obituary.Photo       = inputObituary.Photo;
        obituary.UpdatedAt   = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Obituaries.AnyAsync(e => e.Id == id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // Delete an obituary.
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteObituary(int id)
    {
        var obituary = await _context.Obituaries.FindAsync(id);
        // If obituary not found, return 404
        if (obituary == null)
        {
            return NotFound($"Obituary with ID {id} not found.");
        }

        var userId  = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");

        // If there is no createdByUserId or doesn't match with current user and creator and is not admin will get forbidden.
        if ((string.IsNullOrEmpty(obituary.CreatedByUserId) || obituary.CreatedByUserId != userId) && !isAdmin)
        {
            return Forbid("You can only delete obituaries you created, or you must be an Admin.");
        }

        _context.Obituaries.Remove(obituary);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Get obituaries created by the authenticated user.
    [HttpGet("my-obituaries")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Obituary>>> GetMyObituaries()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token.");
        }

        var items = await _context.Obituaries.Where(o => o.CreatedByUserId == userId).ToListAsync();
        
        return Ok(items);
    }
}
