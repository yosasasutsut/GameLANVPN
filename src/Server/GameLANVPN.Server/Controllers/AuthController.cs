using Microsoft.AspNetCore.Mvc;
using GameLANVPN.Server.Models;
using GameLANVPN.Server.Services;
using System.ComponentModel.DataAnnotations;

namespace GameLANVPN.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingUser = await _userService.GetUserByUsernameAsync(request.Username);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Username already exists" });
            }

            var existingEmail = await _userService.GetUserByEmailAsync(request.Email);
            if (existingEmail != null)
            {
                return BadRequest(new { message = "Email already exists" });
            }

            var user = await _userService.CreateUserAsync(request.Username, request.Email, request.Password);

            _logger.LogInformation("New user registered: {Username}", request.Username);

            return Ok(new
            {
                message = "Registration successful",
                user = new
                {
                    id = user.Id,
                    username = user.Username,
                    email = user.Email,
                    createdAt = user.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userService.ValidateUserAsync(request.Username, request.Password);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            // Update last login
            await _userService.UpdateLastLoginAsync(user.Id);

            _logger.LogInformation("User logged in: {Username}", request.Username);

            return Ok(new
            {
                message = "Login successful",
                user = new
                {
                    id = user.Id,
                    username = user.Username,
                    email = user.Email,
                    lastLogin = DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user login");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("check/{username}")]
    public async Task<IActionResult> CheckUsername(string username)
    {
        try
        {
            var user = await _userService.GetUserByUsernameAsync(username);
            return Ok(new { available = user == null });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking username availability");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}

public class RegisterRequest
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Username can only contain letters, numbers, underscore and dash")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(255, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;

    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}