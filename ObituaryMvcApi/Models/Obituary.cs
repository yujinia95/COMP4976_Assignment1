using System;

namespace ObituaryMvcApi.Models;

public class Obituary
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required DateTime DateOfBirth { get; set; }
    public required DateTime DateOfDeath { get; set; }
    public required string Biography { get; set; } = string.Empty;
    // Foreign key to IdentityUser. Keep non-nullable with default empty string so existing DB schema remains valid.
    public required string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public byte[]? Photo { get; set; } // Optional photo
}
