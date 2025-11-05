namespace ObivtuaryMvcApi.Models;

/// <summary>
/// DTO for creating a new obituary via API
/// </summary>
public class ObituaryCreateDto
{
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DateTime DateOfDeath { get; set; }
    public string Biography { get; set; } = string.Empty;
    public byte[]? Photo { get; set; }
}

/// <summary>
/// DTO for updating an existing obituary via API
/// </summary>
public class ObituaryUpdateDto
{
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DateTime DateOfDeath { get; set; }
    public string Biography { get; set; } = string.Empty;
    public byte[]? Photo { get; set; }
}