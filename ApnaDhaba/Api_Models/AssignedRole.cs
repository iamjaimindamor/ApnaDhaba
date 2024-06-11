namespace ApnaDhaba.Api_Models;

public partial class AssignedRole
{
    public int SerialId { get; set; }

    public int RoleId { get; set; }

    public int UserId { get; set; }

    public virtual RoleModel Role { get; set; } = null!;

    public virtual UserModel User { get; set; } = null!;
}