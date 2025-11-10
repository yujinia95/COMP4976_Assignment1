using System.ComponentModel.DataAnnotations;

namespace ObituaryBlazorWasm.Models.Auth;

/*
    Represents the data required for a user to sign up.
*/
public class SignupModel
{
    [Required, MinLength(3)]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;
}
