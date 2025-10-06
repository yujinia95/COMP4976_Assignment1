using ObituaryApp.Blazor.Models;

namespace ObituaryApp.Blazor.Services;

public interface IObituaryService
{
    Task<List<Obituary>> GetAllObituariesAsync();
    Task<Obituary?> GetObituaryByIdAsync(int id);
    Task<Obituary> CreateObituaryAsync(CreateObituaryDto obituary);
    Task<bool> UpdateObituaryAsync(int id, CreateObituaryDto obituary);
    Task<bool> DeleteObituaryAsync(int id);
    Task<List<Obituary>> SearchObituariesAsync(string searchTerm);
}