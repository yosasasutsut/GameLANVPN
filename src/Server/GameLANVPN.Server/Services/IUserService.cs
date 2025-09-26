using GameLANVPN.Server.Models;

namespace GameLANVPN.Server.Services;

public interface IUserService
{
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(int id);
    Task<User> CreateUserAsync(string username, string email, string password);
    Task<User?> ValidateUserAsync(string username, string password);
    Task UpdateLastLoginAsync(int userId);
    Task<bool> DeleteUserAsync(int userId);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<int> GetUserCountAsync();
}