using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ObituaryApp.Mvc.Models;
using ObituaryApp.Mvc.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace ObituaryApp.Mvc.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        // JWT token service for creating tokens
        private readonly IJwtTokenService _jwt;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger,
            IJwtTokenService jwt)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _jwt = jwt;
        }

        //! Commented out IDK what this code do?
        // [HttpGet]
        // public IActionResult Register()
        // {
        //     return View();
        // }

        /*
            I tried to add JWT token here, but your logic is not letting me return json response.
                - user, result, token are created inside the if and then used outside → out of scope (compiler error).
        
        
        */
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {   

            
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {       
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName, // Map from RegisterViewModel
                    LastName = model.LastName,   // Map from RegisterViewModel
                    Role = "User", // Set default role
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    // Automatically assign "User" role to new users
                    await _userManager.AddToRoleAsync(user, "User");
                    // Also set the custom Role property
                    user.Role = "User";
                    await _userManager.UpdateAsync(user);

                    //! No cookies :)
                    // await _signInManager.SignInAsync(user, isPersistent: false);

                    // Generate JWT token
                    var token = await _jwt.CreateAccessTokenAsync(user);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            //! I need to send json response, but your logic won't let me do it.
            return Created(string.Empty, new
            {
                Message = "User registered successfully",
                user = new
                {
                    user.Id,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                },
                token
            });

            //! We don't need view since this is only for backend API
            // return View(model);
        }



        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return RedirectToLocal(returnUrl);
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    ModelState.AddModelError(string.Empty, "Account locked out.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            return View(user);
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}