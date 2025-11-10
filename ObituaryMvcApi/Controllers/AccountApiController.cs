using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ObituaryMvcApi.Models;

namespace ObituaryMvcApi.Controllers;

/*
    This API controller handles user account operations such as registration and login.
*/
[ApiController]
[Route("api/account")]
public class AccountApiController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _config;
    private readonly ObituaryMvcApi.Services.IJwtTokenService _jwtService;

    // Consturctor using identity services
    public AccountApiController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration config, ObituaryMvcApi.Services.IJwtTokenService jwtService)
    {
        _userManager    = userManager;
        _signInManager  = signInManager;
        _config         = config;
        _jwtService     = jwtService;
    }

    public record LoginRequest(string Email, string Password);
    public record LoginResponse(string Token, DateTime Expires, string? UserId, string? Email);
    public record RegisterRequest(string Username, string Email, string Password);

    /*
        Using POST method to register a new user.
    */
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest model)
    {
        // If required fields are missing (username, email and password), return bad request
        if (string.IsNullOrEmpty(model?.Username) || string.IsNullOrEmpty(model?.Email) || string.IsNullOrEmpty(model?.Password))
        {
            return BadRequest(new { error = "Email and password required" });
        }
        // Check if user with the same email or username already exists
        var existing = await _userManager.FindByEmailAsync(model.Email) ?? await _userManager.FindByNameAsync(model.Username);
        if (existing != null)
        {
            return BadRequest(new { error = "User with that email already exists" });
        }

        // Create new user
        var user = new IdentityUser { UserName = model.Username, Email = model.Email };
        var createResult = await _userManager.CreateAsync(user, model.Password);
        
        // If creation failed, return errors
        if (!createResult.Succeeded)
        {
            var errors = createResult.Errors.Select(e => e.Description).ToArray();
            return BadRequest(new { errors });
        }

        // Assign default role to the new user
        var defaultRole = "User";
        await _userManager.AddToRoleAsync(user, defaultRole);

        var resultBody = new { id = user.Id, email = user.Email, userName = user.UserName };
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, resultBody);
    }



    /*
        Using POST method to login a user and issue a JWT upon successful authentication.
    */
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        // If required fields are missing (email and password), return bad request
        if (string.IsNullOrEmpty(model?.Email) || string.IsNullOrEmpty(model?.Password))
        {
            return BadRequest(new { error = "Email and password required" });
        }

        // Find user by email or username
        var user = await _userManager.FindByEmailAsync(model.Email) ?? await _userManager.FindByNameAsync(model.Email);

        // If user not found, return unauthorized
        if (user == null)
        {
            return Unauthorized(new { error = "Invalid credentials" });
        }

        // Check password 
        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);

        // If password check fails, return unauthorized
        if (!result.Succeeded)
            return Unauthorized(new { error = "Invalid credentials" });

        // Create JWT using the custom jwt service(_jwtService is cusom service)
        var (tokenString, expires) = await _jwtService.CreateTokenAsync(user, _userManager);
        var response = new LoginResponse(tokenString, expires, user.Id, user.Email);

        // Response contains the token, expiration, user ID and email
        return Ok(response);
    }

    // This method to check user details by ID
    // Only Admin role can access this endpoint (for safety reasons?)
    [HttpGet("{id}")]
    [Authorize(Policy = "ApiPolicy", Roles = "Admin")]
    public async Task<IActionResult> GetUser(string id)
    {
        if (string.IsNullOrEmpty(id))
            return BadRequest();

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        var result = new { id = user.Id, email = user.Email, userName = user.UserName };
        return Ok(result);
    }
}
