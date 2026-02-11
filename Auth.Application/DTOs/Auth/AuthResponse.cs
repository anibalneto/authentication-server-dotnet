using Auth.Application.DTOs.User;

namespace Auth.Application.DTOs.Auth;

public class AuthResponse
{
    public UserProfileResponse User { get; set; } = null!;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
