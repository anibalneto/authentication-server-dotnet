using Auth.Application.DTOs.Admin;
using Auth.Application.DTOs.User;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using AutoMapper;

namespace Auth.Application.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;

    public UserService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IAuditLogRepository auditLogRepository,
        IPasswordHasher passwordHasher,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _auditLogRepository = auditLogRepository;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
    }

    public async Task<UserProfileResponse?> GetProfileAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(userId);
        if (user == null) return null;
        return _mapper.Map<UserProfileResponse>(user);
    }

    public async Task<UserProfileResponse?> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(userId);
        if (user == null) return null;

        if (request.FirstName != null) user.FirstName = request.FirstName;
        if (request.LastName != null) user.LastName = request.LastName;

        await _userRepository.UpdateAsync(user);
        return _mapper.Map<UserProfileResponse>(user);
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, string? ipAddress, string? userAgent)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            await LogAuditAsync(userId, "PasswordChange", ipAddress, userAgent, false, "Current password incorrect");
            return false;
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        await _userRepository.UpdateAsync(user);

        await LogAuditAsync(userId, "PasswordChange", ipAddress, userAgent, true);
        return true;
    }

    public async Task<UserListResponse> ListUsersAsync(int page, int pageSize)
    {
        var users = await _userRepository.GetAllAsync(page, pageSize);
        var totalCount = await _userRepository.GetTotalCountAsync();

        return new UserListResponse
        {
            Items = users.Select(u => _mapper.Map<UserProfileResponse>(u)).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<UserDetailResponse?> GetUserByIdAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(userId);
        if (user == null) return null;
        return _mapper.Map<UserDetailResponse>(user);
    }

    public async Task<bool> AssignRoleAsync(Guid userId, string roleName)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(userId);
        if (user == null) return false;

        var role = await _roleRepository.GetByNameAsync(roleName);
        if (role == null) return false;

        if (user.UserRoles.Any(ur => ur.RoleId == role.Id))
        {
            return false;
        }

        user.UserRoles.Add(new UserRole { UserId = userId, RoleId = role.Id });
        await _userRepository.UpdateAsync(user);
        return true;
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
