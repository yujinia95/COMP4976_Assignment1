using System;

namespace ObituaryBlazorWasm.Models;

/*
    Represents an obituary record.
    Why do we need a model in frontend?
        - To structure data received from/sent to the API.
*/
public class Obituary
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; } = DateTime.Today;

    public DateTime DateOfDeath { get; set; } = DateTime.Today;

    public string Biography { get; set; } = string.Empty;

    public string CreatedByUserId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public byte[]? Photo { get; set; }
}
