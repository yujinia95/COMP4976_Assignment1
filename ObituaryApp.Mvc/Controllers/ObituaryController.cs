using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObituaryApp.Mvc.Data;
using ObituaryApp.Mvc.Models;
using SQLitePCL;

namespace ObituaryApp.Mvc.Controllers;


public class ObituaryController : Controller
{

    //! Inject the DbContext (Maybe gotta fix it after db creation)
    private readonly ObituaryDbContext _context;
    public ObituaryController(ObituaryDbContext context)
    {
        _context = context;
    }


    //! Reading all obituaries. Need to work on it.
    // public async Task<IActionResult> Index()
    // {
    //     var obituaries = await _context.Obituaries.ToListAsync();
    //     return Ok(obituaries);
    // }

    // Reading all obituaries via HttpGet method
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var obituaries = await _context.Obituaries.ToListAsync();
        return Ok(obituaries);
    }

    // Reading one obituary by id
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details([FromRoute] int id)
    {
        var obituary = await _context.Obituaries.FirstOrDefaultAsync(o => o.Id == id);
        return obituary is null ? NotFound() : Ok(obituary);
    }

    // Creating a new obituary
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Obituary obituary)
    {
        if (string.IsNullOrWhiteSpace(obituary.FullName))
        {
            return BadRequest("FullName is required.");
        }

        _context.Obituaries.Add(obituary);
        await _context.SaveChangesAsync();

        // This line of code returns a created response
        return CreatedAtAction(nameof(Details), new { id = obituary.Id }, obituary);
    }

    // Editing an existing obituary
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Edit(int id, [FromBody] Obituary input)
    {
        var obituary = await _context.Obituaries.FindAsync(id);
        if (obituary is null)
        {
            return NotFound();
        }

        obituary.FullName = input.FullName;
        obituary.DateOfBirth = input.DateOfBirth;
        obituary.DateOfDeath = input.DateOfDeath;
        obituary.Biography = input.Biography;
        obituary.Photo = input.Photo;
        obituary.PhotoContentType = input.PhotoContentType;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // Deleting an obituary
    [HttpDelete("{id:int}")]    
    public async Task<IActionResult> Delete(int id)
    {
        var obituary = await _context.Obituaries.FindAsync(id);
        if (obituary is null)
        {
            return NotFound();
        }

        _context.Obituaries.Remove(obituary);
        await _context.SaveChangesAsync();
        return NoContent();
    }

}
