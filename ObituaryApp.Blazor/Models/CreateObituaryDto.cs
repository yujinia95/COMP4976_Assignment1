using System.ComponentModel.DataAnnotations;

namespace ObituaryApp.Blazor.Models;

public class CreateObituaryDto
{
    [Required]
    public string FullName { get; set; } = string.Empty;
    
    public DateOnly? DateOfBirth { get; set; }
    
    public DateOnly? DateOfDeath { get; set; }
    
    public string? Biography { get; set; }
    
    public byte[]? Photo { get; set; }
    
    public string? PhotoContentType { get; set; }
}