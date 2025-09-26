using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameLANVPN.Server.Models;

[Table("Users")]
public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Salt { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLogin { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
}

[Table("UserSessions")]
public class UserSession
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [StringLength(255)]
    public string SessionId { get; set; } = string.Empty;

    [StringLength(45)]
    public string? IpAddress { get; set; }

    [StringLength(500)]
    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastActivity { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}