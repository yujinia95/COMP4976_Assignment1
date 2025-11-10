using System.Net;
using System.Net.Http.Json;
using ObituaryBlazorWasm.Models;
using Microsoft.AspNetCore.Components.Forms;
using System.IO;

namespace ObituaryBlazorWasm.Services;

/*
    This class handles obituary-related API calls such as fetching obituaries.
*/
public class ObituaryApiClient
{
    private readonly HttpClient _http;

    // Constructor
    public ObituaryApiClient(HttpClient http)
    {
        _http = http;
    }

    /*
        This method fetches the list of obituaries from the API.
        CancellationToken?
            - Makes token optional because some calls won't need it.
    */
    public async Task<List<Obituary>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _http.GetFromJsonAsync<List<Obituary>>("api/ObituariesApi", cancellationToken: ct);
        return items ?? new List<Obituary>();
    }

    /*
        This method fetches a specific obituary by its ID from the API.
    */
    public async Task<Obituary?> GetAsync(int id, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"api/ObituariesApi/{id}", ct);

        // Handle not found and error responses
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var item = await response.Content.ReadFromJsonAsync<Obituary>(cancellationToken: ct);
        return item;
    }

    /*
        This method creates a new obituary by sending a POST request to the API.
    */
    public async Task<(bool ok, string? error, Obituary? created)> CreateAsync(Obituary model, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/ObituariesApi", model, ct);

        // Handle error response
        if (!response.IsSuccessStatusCode)
        {
            return (false, await response.Content.ReadAsStringAsync(), null);
        }

        var created = await response.Content.ReadFromJsonAsync<Obituary>(cancellationToken: ct);
        return (true, null, created);
    }

    /// <summary>
    /// Create an obituary and optionally include a photo file. The photo is read as a byte[] and assigned to model.Photo.
    /// </summary>
    public async Task<(bool ok, string? error, Obituary? created)> CreateAsync(Obituary model, IBrowserFile? photoFile, CancellationToken ct = default)
    {
        if (photoFile != null)
        {
            model.Photo = await ReadFileAsBytesAsync(photoFile);
        }

        return await CreateAsync(model, ct);
    }

    /*
        This method updates an existing obituary by sending a PUT request to the API.
    */
    public async Task<(bool ok, string? error)> UpdateAsync(int id, Obituary model, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync($"api/ObituariesApi/{id}", model, ct);

        // Handle error response
        if (!response.IsSuccessStatusCode)
        {
            return (false, await response.Content.ReadAsStringAsync());
        }

        return (true, null);
    }

    /// <summary>
    /// Update an obituary; optionally replace the photo with the provided file.
    /// </summary>
    public async Task<(bool ok, string? error)> UpdateAsync(int id, Obituary model, IBrowserFile? photoFile, CancellationToken ct = default)
    {
        if (photoFile != null)
        {
            model.Photo = await ReadFileAsBytesAsync(photoFile);
        }

        return await UpdateAsync(id, model, ct);
    }

    private async Task<byte[]?> ReadFileAsBytesAsync(IBrowserFile file)
    {
        // Limit to 5MB by default to avoid large allocations; adjust as needed
        const long maxBytes = 5 * 1024 * 1024;
        try
        {
            using var stream = file.OpenReadStream(maxBytes);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            return ms.ToArray();
        }
        catch
        {
            // If reading fails (too large or other), return null so caller can handle
            return null;
        }
    }

    /*
        This method deletes an obituary by sending a DELETE request to the API.
    */
    public async Task<(bool ok, string? error)> DeleteAsync(int id, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"api/ObituariesApi/{id}", ct);

        // Handle error response
        if (!response.IsSuccessStatusCode)
        {
            return (false, await response.Content.ReadAsStringAsync());
        }

        return (true, null);
    }

    /*
        This method fetches obituaries created by the current user from the API.
    */
    public async Task<(bool ok, List<Obituary> items, string? error)> GetMine(CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"api/ObituariesApi/my-obituaries", ct);

        // Handle error response
        if (!response.IsSuccessStatusCode)
        {
            return (false, new List<Obituary>(), await response.Content.ReadAsStringAsync());
        }

        var items = await response.Content.ReadFromJsonAsync<List<Obituary>>(cancellationToken: ct) ?? new List<Obituary>();
        return (true, items, null);
    }
}
       