using Auth.Application.DTOs.Auth;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;

namespace Auth.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly TokenService _tokenService;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IAuditLogRepository auditLogRepository,
        IPasswordHasher passwordHasher,
        TokenService tokenService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _auditLogRepository = auditLogRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request, string? ipAddress, string? userAgent)
    {
        if (await _userRepository.ExistsAsync(request.Email))
        {
            await LogAuditAsync(null, "Register", ipAddress, userAgent, false, "Email already registered");
            return null;
        }

        var user = new User
        {
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true,
            IsVerified = false
        };

        var defaultRole = await _roleRepository.GetByNameAsync("User");
        if (defaultRole != null)
        {
            user.UserRoles.Add(new UserRole { Role = defaultRole });
        }

        await _userRepository.CreateAsync(user);

        var userWithRoles = await _userRepository.GetByIdWithRolesAsync(user.Id);
        var roles = userWithRoles!.UserRoles.Select(ur => ur.Role.Name);

        await LogAuditAsync(user.Id, "Register", ipAddress, userAgent, true);

        return await _tokenService.GenerateTokenPairAsync(userWithRoles, roles);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent)
    {
        var user = await _userRepository.GetByEmailWithRolesAsync(request.Email.ToLowerInvariant());
        if (user == null)
        {
            await LogAuditAsync(null, "Login", ipAddress, userAgent, false, "User not found");
            return null;
        }

        if (!user.IsActive)
        {
            await LogAuditAsync(user.Id, "Login", ipAddress, userAgent, false, "Account is inactive");
            return null;
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            await LogAuditAsync(user.Id, "Login", ipAddress, userAgent, false, "Invalid password");
            return null;
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        var roles = user.UserRoles.Select(ur => ur.Role.Name);

        await LogAuditAsync(user.Id, "Login", ipAddress, userAgent, true);

        return await _tokenService.GenerateTokenPairAsync(user, roles);
    }

    public async Task<bool> RequestPasswordResetAsync(string email, string? ipAddress, string? userAgent)
    {
        var user = await _userRepository.GetByEmailAsync(email.ToLowerInvariant());
        if (user != null)
        {
            await LogAuditAsync(user.Id, "PasswordResetRequest", ipAddress, userAgent, true);
        }
        return true;
    }

    public async Task<bool> ResetPasswordAsync(string token, PasswordResetConfirmRequest request, string? ipAddress, string? userAgent)
    {
        await LogAuditAsync(null, "PasswordReset", ipAddress, userAgent, false, "Password reset via token not yet implemented");
        return false;
    }

    private async Task LogAuditAsync(Guid? userId, string action, string? ipAddress, string? userAgent, bool success, string? errorMessage = null)
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = action,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Success = success,
            ErrorMessage = errorMessage
        };

        await _auditLogRepository.CreateAsync(auditLog);
    }
}
