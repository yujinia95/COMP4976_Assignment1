using System.ComponentModel.DataAnnotations;

namespace ObituaryBlazorWasm.Models.Auth;

/*
    Represents the data required for a user to log in.
*/
public class LoginModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/*
    Represents the result of a login attempt.
*/
public class LoginResult
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
    public string? UserId { get; set; }
    public string? Email { get; set; }
}
