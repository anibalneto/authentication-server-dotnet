using Auth.Application.DTOs.User;

namespace Auth.Application.DTOs.Auth;

public class TokenVerifyResponse
{
    public bool Valid { get; set; }
    public UserProfileResponse? User { get; set; }
}
