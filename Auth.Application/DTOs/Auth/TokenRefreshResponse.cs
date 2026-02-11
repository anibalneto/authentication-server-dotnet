namespace Auth.Application.DTOs.Auth;

public class TokenRefreshResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
