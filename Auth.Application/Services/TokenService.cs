using System.Security.Cryptography;
using Auth.Application.DTOs.Auth;
using Auth.Application.DTOs.User;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using AutoMapper;

namespace Auth.Application.Services;

public class TokenService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IMapper _mapper;

    public TokenService(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IMapper mapper)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _mapper = mapper;
    }

    public virtual async Task<AuthResponse> GenerateTokenPairAsync(User user, IEnumerable<string> roles)
    {
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles);
        var refreshToken = await CreateRefreshTokenAsync(user.Id);

        return new AuthResponse
        {
            User = _mapper.Map<UserProfileResponse>(user),
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token
        };
    }

    public virtual async Task<TokenRefreshResponse?> RefreshTokenAsync(string token)
    {
        var existingToken = await _refreshTokenRepository.GetByTokenAsync(token);
        if (existingToken == null || existingToken.IsRevoked || existingToken.ExpiresAt <= DateTime.UtcNow)
        {
            return null;
        }

        existingToken.IsRevoked = true;
        await _refreshTokenRepository.UpdateAsync(existingToken);

        var user = await _userRepository.GetByIdWithRolesAsync(existingToken.UserId);
        if (user == null || !user.IsActive)
        {
            return null;
        }

        var roles = user.UserRoles.Select(ur => ur.Role.Name);
        var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles);
        var newRefreshToken = await CreateRefreshTokenAsync(user.Id);

        return new TokenRefreshResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token
        };
    }

    public virtual async Task<TokenVerifyResponse> VerifyTokenAsync(string accessToken, Guid userId)
    {
        var isValid = _jwtTokenGenerator.ValidateToken(accessToken);
        if (!isValid)
        {
            return new TokenVerifyResponse { Valid = false };
        }

        var user = await _userRepository.GetByIdWithRolesAsync(userId);
        if (user == null)
        {
            return new TokenVerifyResponse { Valid = false };
        }

        return new TokenVerifyResponse
        {
            Valid = true,
            User = _mapper.Map<UserProfileResponse>(user)
        };
    }

    public virtual async Task RevokeAllTokensAsync(Guid userId)
    {
        await _refreshTokenRepository.RevokeAllForUserAsync(userId);
    }

    private async Task<RefreshToken> CreateRefreshTokenAsync(Guid userId)
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(64);
        var tokenString = Convert.ToBase64String(tokenBytes);

        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = tokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        await _refreshTokenRepository.CreateAsync(refreshToken);
        return refreshToken;
    }
}
