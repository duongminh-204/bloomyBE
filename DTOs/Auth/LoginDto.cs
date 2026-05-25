namespace Bloomy.DTOs.Auth;

public class LoginDto
{
    public string EmailOrPhone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}