using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ObituaryApp.Blazor.Models;

namespace ObituaryApp.Blazor.Services;

public class ObituaryService : IObituaryService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;
    private readonly string _baseUrl = "api/obituary";

    public ObituaryService(HttpClient httpClient, IAuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    public async Task<List<Obituary>> GetAllObituariesAsync()
    {
        try
        {
            var obituaries = await _httpClient.GetFromJsonAsync<List<Obituary>>(_baseUrl);
            return obituaries ?? new List<Obituary>();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Network error fetching obituaries: {ex.Message}");
            // Return mock data for development/testing when API is not available
            return GetMockObituaries();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching obituaries: {ex.Message}");
            return new List<Obituary>();
        }
    }

    public async Task<Obituary?> GetObituaryByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Obituary>($"{_baseUrl}/{id}");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Network error fetching obituary {id}: {ex.Message}");
            // Return mock data for development when API is not available
            var mockObituaries = GetMockObituaries();
            return mockObituaries.FirstOrDefault(o => o.Id == id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching obituary {id}: {ex.Message}");
            return null;
        }
    }

    public async Task<Obituary> CreateObituaryAsync(CreateObituaryDto obituary)
    {
        await SetAuthorizationHeaderAsync();
        
        try
        {
            var json = JsonSerializer.Serialize(obituary);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(_baseUrl, content);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Obituary>();
                return result!;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("You must be logged in to create obituaries");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to create obituary: {response.StatusCode} - {errorContent}");
            }
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Network error creating obituary: {ex.Message}");
        }
    }

    public async Task<bool> UpdateObituaryAsync(int id, CreateObituaryDto obituary)
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            
            var json = JsonSerializer.Serialize(obituary);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync($"{_baseUrl}/{id}", content);
            
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("Unauthorized: You don't have permission to edit this obituary");
                return false;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine("Obituary not found");
                return false;
            }
            else
            {
                Console.WriteLine($"Update failed: {response.StatusCode}");
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Network error updating obituary: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating obituary: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteObituaryAsync(int id)
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("Unauthorized: You don't have permission to delete this obituary");
                return false;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine("Obituary not found");
                return false;
            }
            else
            {
                Console.WriteLine($"Delete failed: {response.StatusCode}");
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Network error deleting obituary: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting obituary: {ex.Message}");
            return false;
        }
    }

    public async Task<List<Obituary>> SearchObituariesAsync(string searchTerm)
    {
        try
        {
            var obituaries = await GetAllObituariesAsync();
            return obituaries.Where(o => o.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                           .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching obituaries: {ex.Message}");
            return new List<Obituary>();
        }
    }

    private async Task SetAuthorizationHeaderAsync()
    {
        var token = await _authService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }

    // Mock data for development when API is not available
    private List<Obituary> GetMockObituaries()
    {
        return new List<Obituary>
        {
            new Obituary
            {
                Id = 1,
                FullName = "John Smith",
                DateOfBirth = new DateOnly(1940, 5, 15),
                DateOfDeath = new DateOnly(2023, 8, 20),
                Biography = "John was a loving father and grandfather who dedicated his life to teaching. He touched the lives of countless students over his 40-year career."
            },
            new Obituary
            {
                Id = 2,
                FullName = "Mary Johnson",
                DateOfBirth = new DateOnly(1955, 12, 3),
                DateOfDeath = new DateOnly(2024, 1, 10),
                Biography = "Mary was a passionate artist and community volunteer. Her paintings brought joy to many, and her charitable work made a lasting impact on our community."
            },
            new Obituary
            {
                Id = 3,
                FullName = "Robert Williams",
                DateOfBirth = new DateOnly(1932, 7, 22),
                DateOfDeath = new DateOnly(2023, 11, 5),
                Biography = "Robert served his country with honor in the military and later became a successful businessman. He was known for his integrity and generosity."
            }
        };
    }
}