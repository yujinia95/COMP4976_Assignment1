using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ObituaryMvcApi.Data;

namespace ServerMCP.Services;

public class ObituaryService(ApplicationDbContext db)
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<string> GetAllObituariesJson()
    {
        var obituaries = await db.Obituaries.ToListAsync();
        return JsonSerializer.Serialize(obituaries, _jsonOptions);
    }

    public async Task<string> GetObituaryByIdJson(int id)
    {
        var obituary = await db.Obituaries.FindAsync(id);
        return JsonSerializer.Serialize(obituary, _jsonOptions);
    }

    public async Task<string> GetObituariesByNameJson(string name)
    {
        var obituaries = await db.Obituaries
            .Where(o => o.FullName.Contains(name))
            .ToListAsync();

        return JsonSerializer.Serialize(obituaries, _jsonOptions);
    }

    public async Task<string> GetObituariesByBiographyKeywordJson(string keyword)
    {
        var obituaries = await db.Obituaries
            .Where(o => o.Biography.Contains(keyword))
            .ToListAsync();

        return JsonSerializer.Serialize(obituaries, _jsonOptions);
    }

    public async Task<string> GetObituariesBornBeforeJson(DateTime date)
    {
        var obituaries = await db.Obituaries
            .Where(o => o.DateOfBirth < date)
            .ToListAsync();

        return JsonSerializer.Serialize(obituaries, _jsonOptions);
    }

    public async Task<string> GetObituariesDiedAfterJson(DateTime date)
    {
        var obituaries = await db.Obituaries
            .Where(o => o.DateOfDeath > date)
            .ToListAsync();

        return JsonSerializer.Serialize(obituaries, _jsonOptions);
    }

    public async Task<string> GetObituariesCreatedByUserJson(string userId)
    {
        var obituaries = await db.Obituaries
            .Where(o => o.CreatedByUserId == userId)
            .ToListAsync();

        return JsonSerializer.Serialize(obituaries, _jsonOptions);
    }
}
