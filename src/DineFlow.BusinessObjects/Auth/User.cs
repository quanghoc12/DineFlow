using DineFlow.BusinessObjects.Common;

namespace DineFlow.BusinessObjects.Auth;

public class User : BaseEntity
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Staff;
    public bool IsActive { get; set; } = true;
}
