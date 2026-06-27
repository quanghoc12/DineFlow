namespace DineFlow.BusinessObjects.Auth.DTOs;

public class UpdateUserRequestDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int RoleId { get; set; }
}
