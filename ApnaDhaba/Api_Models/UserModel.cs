using System.ComponentModel.DataAnnotations;

namespace ApnaDhaba.Api_Models;

public class UserModel
{
    [Key]
    public int userId { get; set; }

    [Required]
    public string? Firstname { get; set; }

    [Required]
    public string? Lastname { get; set; }

    public string? Email { get; set; } = null;

    [Required]
    public string? Username { get; set; }

    [Required]
    public string? Password { get; set; }

    public string? ImageURL { get; set; } = null;

    [Required]
    public DateTime? CreateDate { get; set; }

    public DateTime? LastModifiedDate { get; set; }
    public string? Address { get; set; }

    public virtual ICollection<AssignedRole> AssignedRoles { get; set; } = new List<AssignedRole>();
}