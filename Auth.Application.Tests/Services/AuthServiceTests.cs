using Auth.Application.DTOs.Auth;
using Auth.Application.DTOs.User;
using Auth.Application.Services;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Moq;

namespace Auth.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IRoleRepository> _roleRepo = new();
    private readonly Mock<IAuditLogRepository> _auditRepo = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<TokenService> _tokenService;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var refreshTokenRepo = new Mock<IRefreshTokenRepository>();
        var jwtGenerator = new Mock<IJwtTokenGenerator>();
        var mapper = new Mock<AutoMapper.IMapper>();

        _tokenService = new Mock<TokenService>(
            refreshTokenRepo.Object,
            _userRepo.Object,
            jwtGenerator.Object,
            mapper.Object);

        _authService = new AuthService(
            _userRepo.Object,
            _roleRepo.Object,
            _auditRepo.Object,
            _passwordHasher.Object,
            _tokenService.Object);
    }

    [Fact]
    public async Task Register_Returns_Null_When_Email_Already_Exists()
    {
        _userRepo.Setup(x => x.ExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

        var request = new RegisterRequest { Email = "exists@test.com", Password = "Password1!" };
        var result = await _authService.RegisterAsync(request, "127.0.0.1", "test-agent");

        Assert.Null(result);
    }

    [Fact]
    public async Task Register_Creates_User_And_Returns_Tokens()
    {
        var userId = Guid.NewGuid();
        var role = new Role { Id = Guid.NewGuid(), Name = "User" };

        _userRepo.Setup(x => x.ExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _passwordHasher.Setup(x => x.HashPassword(It.IsAny<string>())).Returns("hashed");
        _roleRepo.Setup(x => x.GetByNameAsync("User")).ReturnsAsync(role);
        _userRepo.Setup(x => x.CreateAsync(It.IsAny<User>())).ReturnsAsync((User u) => { u.Id = userId; return u; });

        var userWithRoles = new User
        {
            Id = userId,
            Email = "new@test.com",
            UserRoles = new List<UserRole> { new() { Role = role } }
        };
        _userRepo.Setup(x => x.GetByIdWithRolesAsync(It.IsAny<Guid>())).ReturnsAsync(userWithRoles);

        var expectedResponse = new AuthResponse
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            User = new UserProfileResponse { Id = userId, Email = "new@test.com" }
        };
        _tokenService.Setup(x => x.GenerateTokenPairAsync(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(expectedResponse);

        var request = new RegisterRequest { Email = "new@test.com", Password = "Password1!", FirstName = "John" };
        var result = await _authService.RegisterAsync(request, "127.0.0.1", "test-agent");

        Assert.NotNull(result);
        Assert.Equal("access-token", result.AccessToken);
        _userRepo.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task Login_Returns_Null_When_User_Not_Found()
    {
        _userRepo.Setup(x => x.GetByEmailWithRolesAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        var request = new LoginRequest { Email = "notfound@test.com", Password = "Password1!" };
        var result = await _authService.LoginAsync(request, "127.0.0.1", "test-agent");

        Assert.Null(result);
    }

    [Fact]
    public async Task Login_Returns_Null_When_User_Inactive()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "test@test.com", IsActive = false, PasswordHash = "hash" };
        _userRepo.Setup(x => x.GetByEmailWithRolesAsync(It.IsAny<string>())).ReturnsAsync(user);

        var request = new LoginRequest { Email = "test@test.com", Password = "Password1!" };
        var result = await _authService.LoginAsync(request, "127.0.0.1", "test-agent");

        Assert.Null(result);
    }

    [Fact]
    public async Task Login_Returns_Null_When_Password_Invalid()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com",
            IsActive = true,
            PasswordHash = "hash",
            UserRoles = new List<UserRole>()
        };
        _userRepo.Setup(x => x.GetByEmailWithRolesAsync(It.IsAny<string>())).ReturnsAsync(user);
        _passwordHasher.Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var request = new LoginRequest { Email = "test@test.com", Password = "WrongPass1!" };
        var result = await _authService.LoginAsync(request, "127.0.0.1", "test-agent");

        Assert.Null(result);
    }

    [Fact]
    public async Task Login_Returns_Tokens_When_Credentials_Valid()
    {
        var role = new Role { Id = Guid.NewGuid(), Name = "User" };
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com",
            IsActive = true,
            PasswordHash = "hash",
            UserRoles = new List<UserRole> { new() { Role = role } }
        };

        _userRepo.Setup(x => x.GetByEmailWithRolesAsync(It.IsAny<string>())).ReturnsAsync(user);
        _passwordHasher.Setup(x => x.VerifyPassword("Password1!", "hash")).Returns(true);

        var expectedResponse = new AuthResponse
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            User = new UserProfileResponse { Id = user.Id, Email = user.Email }
        };
        _tokenService.Setup(x => x.GenerateTokenPairAsync(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(expectedResponse);

        var request = new LoginRequest { Email = "test@test.com", Password = "Password1!" };
        var result = await _authService.LoginAsync(request, "127.0.0.1", "test-agent");

        Assert.NotNull(result);
        Assert.Equal("access-token", result.AccessToken);
    }
}
