using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;
using ServerMCP.Data;
using ObituaryMvcApi.Models;

namespace ServerMCP.McpTools;

[McpServerToolType]
public sealed class ObituaryTool
{
    private readonly ApplicationDbContext _context;

    public ObituaryTool(ApplicationDbContext context)
    {
        _context = context;
    }

    [McpServerTool, Description("Get all obituaries from the database")]
    public async Task<string> GetAllObituaries()
    {
        var obituaries = await _context.Obituaries
            .OrderByDescending(o => o.DateOfDeath)
            .ToListAsync();

        if (!obituaries.Any())
        {
            return "No obituaries found in the database.";
        }

        var result = $"Found {obituaries.Count} obituaries:\n\n";
        foreach (var obit in obituaries)
        {
            result += $"- {obit.FullName} ({obit.DateOfBirth:d} - {obit.DateOfDeath:d})\n";
            result += $"  Created: {obit.CreatedAt:g}\n";
            if (!string.IsNullOrEmpty(obit.Biography))
            {
                var shortBio = obit.Biography.Length > 100
                    ? obit.Biography.Substring(0, 100) + "..."
                    : obit.Biography;
                result += $"  Biography: {shortBio}\n";
            }
            result += "\n";
        }

        return result;
    }

    [McpServerTool, Description("Search for obituaries by deceased person's name")]
    public async Task<string> SearchObituariesByName(string name)
    {
        var obituaries = await _context.Obituaries
            .Where(o => o.FullName.Contains(name))
            .OrderByDescending(o => o.DateOfDeath)
            .ToListAsync();

        if (!obituaries.Any())
        {
            return $"No obituaries found for '{name}'.";
        }

        var result = $"Found {obituaries.Count} obituary(ies) matching '{name}':\n\n";
        foreach (var obit in obituaries)
        {
            result += $"- {obit.FullName} ({obit.DateOfBirth:d} - {obit.DateOfDeath:d})\n";
            result += $"  Created: {obit.CreatedAt:g}\n";
            if (!string.IsNullOrEmpty(obit.Biography))
            {
                result += $"  Biography: {obit.Biography}\n";
            }
            result += "\n";
        }

        return result;
    }

    [McpServerTool, Description("Get detailed information about a specific obituary by ID")]
    public async Task<string> GetObituaryById(int obituaryId)
    {
        var obituary = await _context.Obituaries.FindAsync(obituaryId);

        if (obituary == null)
        {
            return $"No obituary found with ID {obituaryId}.";
        }

        var result = $"Obituary Details:\n";
        result += $"ID: {obituary.Id}\n";
        result += $"Full Name: {obituary.FullName}\n";
        result += $"Date of Birth: {obituary.DateOfBirth:D}\n";
        result += $"Date of Death: {obituary.DateOfDeath:D}\n";
        result += $"Created By User ID: {obituary.CreatedByUserId}\n";
        result += $"Created At: {obituary.CreatedAt:g}\n";
        result += $"Updated At: {obituary.UpdatedAt:g}\n";

        if (!string.IsNullOrEmpty(obituary.Biography))
        {
            result += $"\nBiography:\n{obituary.Biography}\n";
        }

        if (obituary.Photo != null && obituary.Photo.Length > 0)
        {
            result += $"\nPhoto: Yes (size: {obituary.Photo.Length} bytes)\n";
        }

        return result;
    }

    [McpServerTool, Description("Get the most recent obituary (person who died most recently)")]
    public async Task<string> GetMostRecentDeath()
    {
        var obituary = await _context.Obituaries
            .OrderByDescending(o => o.DateOfDeath)
            .FirstOrDefaultAsync();

        if (obituary == null)
            return "No obituaries found.";

        return $"The most recent death is {obituary.FullName}, who passed away on {obituary.DateOfDeath:D}.";
    }

    [McpServerTool, Description("Get the oldest person based on age at death")]
    public async Task<string> GetOldestPerson()
    {
        var obituary = await _context.Obituaries
            .OrderByDescending(o => EF.Functions.DateDiffYear(o.DateOfBirth, o.DateOfDeath))
            .FirstOrDefaultAsync();

        if (obituary == null)
            return "No obituaries found.";

        var age = obituary.DateOfDeath.Year - obituary.DateOfBirth.Year;
        if (obituary.DateOfDeath < obituary.DateOfBirth.AddYears(age)) age--;

        return $"{obituary.FullName} was the oldest, passing away at approximately {age} years old.";
    }

    [McpServerTool, Description("List obituaries for people who died between two dates")]
    public async Task<string> GetDeathsBetween(DateTime start, DateTime end)
    {
        var obituaries = await _context.Obituaries
            .Where(o => o.DateOfDeath >= start && o.DateOfDeath <= end)
            .OrderBy(o => o.DateOfDeath)
            .ToListAsync();

        if (!obituaries.Any())
            return $"No deaths found between {start:D} and {end:D}.";

        var result = $"Obituaries between {start:D} and {end:D}:\n\n";
        foreach (var o in obituaries)
            result += $"- {o.FullName} ({o.DateOfDeath:D})\n";

        return result;
    }

    [McpServerTool, Description("Get the person with the earliest date of birth")]
    public async Task<string> GetEarliestBirth()
    {
        var obituary = await _context.Obituaries
            .OrderBy(o => o.DateOfBirth)
            .FirstOrDefaultAsync();

        if (obituary == null)
            return "No obituary records found.";

        return $"{obituary.FullName} was born on {obituary.DateOfBirth:D}, making them the earliest born person in the database.";
    }

    [McpServerTool, Description("Search obituaries by biography keyword, e.g. hobbies or interests")]
    public async Task<string> SearchObituariesByBiographyKeyword(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return "Please provide a keyword to search for (e.g. 'gardening', 'teacher', 'musician').";

        var results = await _context.Obituaries
            .Where(o => o.Biography.Contains(keyword))
            .OrderByDescending(o => o.DateOfDeath)
            .ToListAsync();

        if (!results.Any())
            return $"No obituaries mention '{keyword}' in their biographies.";

        var response = $"Found {results.Count} obituary(ies) mentioning '{keyword}':\n\n";

        foreach (var o in results)
        {
            var snippet = o.Biography.Length > 120 
                ? o.Biography[..120] + "..." 
                : o.Biography;

            response += $"- {o.FullName} ({o.DateOfBirth:d} - {o.DateOfDeath:d})\n";
            response += $"  Biography snippet: {snippet}\n\n";
        }

        return response;
    }

}

