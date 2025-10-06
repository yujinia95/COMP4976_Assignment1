using System.ComponentModel.DataAnnotations;

namespace ObituaryApp.Blazor.Models;

public class Obituary
{
    public int Id { get; set; }
    
    [Required]
    public string FullName { get; set; } = string.Empty;
    
    public DateOnly? DateOfBirth { get; set; }
    
    public DateOnly? DateOfDeath { get; set; }
    
    public string? Biography { get; set; }
    
    public byte[]? Photo { get; set; }
    
    public string? PhotoContentType { get; set; }
    
    // Helper property for displaying photo
    public string? PhotoDataUrl
    {
        get
        {
            if (Photo != null && !string.IsNullOrEmpty(PhotoContentType))
            {
                var base64String = Convert.ToBase64String(Photo);
                return $"data:{PhotoContentType};base64,{base64String}";
            }
            return null;
        }
    }
}