namespace DineFlow.BusinessObjects.Auth.DTOs;

public class ResetPasswordRequestDto
{
    public int UserId { get; set; }
    public string NewPassword { get; set; } = string.Empty;
}
