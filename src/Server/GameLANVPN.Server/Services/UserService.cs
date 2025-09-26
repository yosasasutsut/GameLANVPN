using GameLANVPN.Server.Data;
using GameLANVPN.Server.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace GameLANVPN.Server.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(ApplicationDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
    }

    public async Task<User> CreateUserAsync(string username, string email, string password)
    {
        var salt = GenerateSalt();
        var passwordHash = HashPassword(password, salt);

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            Salt = salt,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User created successfully: {Username}", username);
        return user;
    }

    public async Task<User?> ValidateUserAsync(string username, string password)
    {
        var user = await GetUserByUsernameAsync(username);
        if (user == null)
        {
            return null;
        }

        var passwordHash = HashPassword(password, user.Salt);
        if (passwordHash != user.PasswordHash)
        {
            return null;
        }

        return user;
    }

    public async Task UpdateLastLoginAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> DeleteUserAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.IsActive = false;
        await _context.SaveChangesAsync();

        _logger.LogInformation("User deactivated: {UserId}", userId);
        return true;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _context.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.Username)
            .ToListAsync();
    }

    public async Task<int> GetUserCountAsync()
    {
        return await _context.Users
            .CountAsync(u => u.IsActive);
    }

    private static string GenerateSalt()
    {
        using var rng = RandomNumberGenerator.Create();
        var saltBytes = new byte[32];
        rng.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }

    private static string HashPassword(string password, string salt)
    {
        using var sha256 = SHA256.Create();
        var passwordBytes = Encoding.UTF8.GetBytes(password + salt);
        var hashBytes = sha256.ComputeHash(passwordBytes);
        return Convert.ToBase64String(hashBytes);
    }
}