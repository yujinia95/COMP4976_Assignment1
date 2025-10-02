using Microsoft.AspNetCore.Identity;

namespace ObituaryApp.Mvc.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Additional properties not in IdentityUser
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}