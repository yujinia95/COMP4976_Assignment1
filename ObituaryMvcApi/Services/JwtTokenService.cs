using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ObituaryMvcApi.Services;

/*
    Service for creating JWT tokens for authenticated users.
    This is custom service to encapsulate JWT token generation logic.
    Why made separate service?
    - Keeps token generation logic separate from controllers.
*/
public interface IJwtTokenService
{
    Task<(string token, DateTime expires)> CreateTokenAsync(IdentityUser user, UserManager<IdentityUser> userManager);
}

/*
    Implementation of the JWT token creation service.
*/
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _config;

    /*
        Constructor to initialize configuration.
    */
    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    /*
        Creates a JWT token for the specified user including their roles as claims.
    */
    public async Task<(string token, DateTime expires)> CreateTokenAsync(IdentityUser user, UserManager<IdentityUser> userManager)
    {
        // Read JWT settings from configuration
        var jwtSection    = _config.GetSection("Jwt");
        var key           = jwtSection.GetValue<string>("Key") ?? string.Empty;
        var issuer        = jwtSection.GetValue<string>("Issuer");
        var audience      = jwtSection.GetValue<string>("Audience");
        var expireMinutes = jwtSection.GetValue<int>("ExpireMinutes");

        var keyBytes = Encoding.UTF8.GetBytes(key);

        // Create claims for the token
        // What is claims?
        //      - Pieces of information about the user (like user ID, email, roles) that are encoded in the token.
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? user.Email ?? string.Empty),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
        };
        var roles = await userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Create signing credentials using the secret key and algorithm
        var signingKey = new SymmetricSecurityKey(keyBytes);
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        // Set token expiration time (fixed to 60 minutes)
        var expires = DateTime.UtcNow.AddMinutes(60);

        // Create the JWT token object 
        var token = new JwtSecurityToken(
            issuer            : issuer,
            audience          : audience,
            claims            : claims,
            expires           : expires,
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenString, expires);
    }
}
