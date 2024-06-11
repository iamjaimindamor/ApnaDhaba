namespace ApnaDhaba.Api_Models;

public partial class RoleModel
{
    public int RoleId { get; set; }

    public string? RoleName { get; set; }

    public virtual ICollection<AssignedRole> AssignedRoles { get; set; } = new List<AssignedRole>();
}