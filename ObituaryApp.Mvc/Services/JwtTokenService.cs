using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using ObituaryApp.Mvc.Models;

/*
    IJwtTokenService defines methods for creating JWT tokens.
*/
public interface IJwtTokenService
{
    // This method creates a JWT access token for the given user
    Task<string> CreateAccessTokenAsync(ApplicationUser user);
}

/*
    JwtTokenService implements IJwtTokenService to generate JWT tokens.
    This class is the actual implementation of the token creation logic.
*/
public class JwtTokenService : IJwtTokenService
{
    // This config is used for reading JWT settings from appsettings.json
    private readonly IConfiguration _config;
    // This userManager is used for accessing user info and roles.
    private readonly UserManager<ApplicationUser> _userManager;


    // Constructor
    public JwtTokenService(IConfiguration config, UserManager<ApplicationUser> userManager)
    {
        _config = config;
        _userManager = userManager;
    }


    /*
        This method creates a JWT access token for the given user.
    */
    public async Task<string> CreateAccessTokenAsync(ApplicationUser user)
    {
        // Read JWT settings from appsettings.json
        var jwt = _config.GetSection("Jwt");

        // Create security key. This key will be used to sign and verify the token.
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

        // Set algorithm to use for singing the token
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var roles = await _userManager.GetRolesAsync(user);


        // This block builds a list of claims with infos we want to include in the token
        var claims = new List<Claim>
        {   
            // Identifies who the token is for (sub = subject)
            new(JwtRegisteredClaimNames.Sub, user.Id),
            // NameIdentifier helps to identify the user later. (Used in ObituaryController)
            new(ClaimTypes.NameIdentifier, user.Id),
            // Store username. Could be email if username is not set.
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty),
            // Stroe the user's email
            new(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        // Add roles in the list of claims. (Just like the code above line 55-65)
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Create the JWT token with the given claims and settings. (expires in 15 minutes)
        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials
        );

        // Return the serialized token as a string. (Header.Payload.Signature)
        return new JwtSecurityTokenHandler().WriteToken(token);
    }







}