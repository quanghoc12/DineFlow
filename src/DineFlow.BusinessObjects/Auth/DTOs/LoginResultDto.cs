namespace DineFlow.BusinessObjects.Auth.DTOs;

public class LoginResultDto
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public CurrentUserDto? User { get; set; }
}
