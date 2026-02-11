using Auth.Domain.Entities;

namespace Auth.Domain.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user, IEnumerable<string> roles);
    bool ValidateToken(string token);
    Guid? GetUserIdFromToken(string token);
}
