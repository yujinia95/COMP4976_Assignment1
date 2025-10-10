using System.ComponentModel.DataAnnotations;

namespace ObivtuaryMvcApi.Models;

public class ObituaryCreateViewModel : IValidatableObject
{
    [Required]
    [Display(Name = "Full Name")]
    [StringLength(200, ErrorMessage = "Full name cannot exceed 200 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [Required]
    [Display(Name = "Date of Death")]
    [DataType(DataType.Date)]
    public DateTime DateOfDeath { get; set; }

    [Required]
    [Display(Name = "Biography")]
    [StringLength(5000, ErrorMessage = "Biography cannot exceed 5000 characters.")]
    public string Biography { get; set; } = string.Empty;

    [Display(Name = "Photo")]
    public IFormFile? PhotoFile { get; set; }

    // Custom validation to ensure DateOfDeath is after or equal to DateOfBirth
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DateOfDeath < DateOfBirth)
        {
            yield return new ValidationResult("Date of Death cannot be before Date of Birth.", new[] { nameof(DateOfDeath) });
        }

        if (DateOfBirth > DateTime.Now)
        {
            yield return new ValidationResult("Date of Birth cannot be in the future.", new[] { nameof(DateOfBirth) });
        }

        if (DateOfDeath > DateTime.Now)
        {
            yield return new ValidationResult("Date of Death cannot be in the future.", new[] { nameof(DateOfDeath) });
        }
    }
}

public class ObituaryEditViewModel : IValidatableObject
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Full Name")]
    [StringLength(200, ErrorMessage = "Full name cannot exceed 200 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [Required]
    [Display(Name = "Date of Death")]
    [DataType(DataType.Date)]
    public DateTime DateOfDeath { get; set; }

    [Required]
    [Display(Name = "Biography")]
    [StringLength(5000, ErrorMessage = "Biography cannot exceed 5000 characters.")]
    public string Biography { get; set; } = string.Empty;

    [Display(Name = "Photo")]
    public IFormFile? PhotoFile { get; set; }

    public bool HasExistingPhoto { get; set; }

    // Custom validation to ensure DateOfDeath is after or equal to DateOfBirth
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DateOfDeath < DateOfBirth)
        {
            yield return new ValidationResult("Date of Death cannot be before Date of Birth.", new[] { nameof(DateOfDeath) });
        }

        if (DateOfBirth > DateTime.Now)
        {
            yield return new ValidationResult("Date of Birth cannot be in the future.", new[] { nameof(DateOfBirth) });
        }

        if (DateOfDeath > DateTime.Now)
        {
            yield return new ValidationResult("Date of Death cannot be in the future.", new[] { nameof(DateOfDeath) });
        }
    }
}
public class ObituaryDeleteViewModel
{
    public Obituary Obituary { get; set; } = new Obituary
    {
        FullName = string.Empty,
        DateOfBirth = DateTime.MinValue,
        DateOfDeath = DateTime.MinValue,
        Biography = string.Empty,
        CreatedByUserId = string.Empty,
        CreatedAt = DateTime.MinValue,
        UpdatedAt = DateTime.MinValue
    };
    public bool CanDelete { get; set; }
}
