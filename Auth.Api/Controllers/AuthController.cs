using System.Security.Claims;
using Auth.Application.DTOs.Auth;
using Auth.Application.DTOs.Common;
using Auth.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly TokenService _tokenService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<PasswordResetConfirmRequest> _passwordResetValidator;

    public AuthController(
        AuthService authService,
        TokenService tokenService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        IValidator<PasswordResetConfirmRequest> passwordResetValidator)
    {
        _authService = authService;
        _tokenService = tokenService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _passwordResetValidator = passwordResetValidator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var validation = await _registerValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return BadRequest(ApiErrorResponse.Create("VALIDATION_ERROR", "Validation failed",
                validation.Errors.Select(e => e.ErrorMessage)));
        }

        var result = await _authService.RegisterAsync(request, GetIpAddress(), GetUserAgent());
        if (result == null)
        {
            return Conflict(ApiErrorResponse.Create("EMAIL_EXISTS", "Email is already registered"));
        }

        return StatusCode(201, ApiResponse<AuthResponse>.SuccessResponse(result, "User registered successfully"));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var validation = await _loginValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return BadRequest(ApiErrorResponse.Create("VALIDATION_ERROR", "Validation failed",
                validation.Errors.Select(e => e.ErrorMessage)));
        }

        var result = await _authService.LoginAsync(request, GetIpAddress(), GetUserAgent());
        if (result == null)
        {
            return Unauthorized(ApiErrorResponse.Create("INVALID_CREDENTIALS", "Invalid email or password"));
        }

        return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Login successful"));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        await _tokenService.RevokeAllTokensAsync(userId.Value);
        return Ok(ApiResponse.SuccessResponse("Logout successful"));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var result = await _tokenService.RefreshTokenAsync(request.RefreshToken);
        if (result == null)
        {
            return Unauthorized(ApiErrorResponse.Create("INVALID_TOKEN", "Invalid or expired refresh token"));
        }

        return Ok(ApiResponse<TokenRefreshResponse>.SuccessResponse(result, "Token refreshed successfully"));
    }

    [Authorize]
    [HttpPost("verify")]
    public async Task<IActionResult> Verify()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var result = await _tokenService.VerifyTokenAsync(token, userId.Value);

        return Ok(ApiResponse<TokenVerifyResponse>.SuccessResponse(result));
    }

    [HttpPost("password-reset")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequest request)
    {
        await _authService.RequestPasswordResetAsync(request.Email, GetIpAddress(), GetUserAgent());
        return Ok(ApiResponse.SuccessResponse("If the email exists, a password reset link has been sent"));
    }

    [HttpPost("password-reset/{token}")]
    public async Task<IActionResult> ResetPassword(string token, [FromBody] PasswordResetConfirmRequest request)
    {
        var validation = await _passwordResetValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return BadRequest(ApiErrorResponse.Create("VALIDATION_ERROR", "Validation failed",
                validation.Errors.Select(e => e.ErrorMessage)));
        }

        var result = await _authService.ResetPasswordAsync(token, request, GetIpAddress(), GetUserAgent());
        if (!result)
        {
            return BadRequest(ApiErrorResponse.Create("INVALID_TOKEN", "Invalid or expired reset token"));
        }

        return Ok(ApiResponse.SuccessResponse("Password reset successful"));
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
            ?? User.FindFirst("sub");
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return null;
    }

    private string? GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString();
    private string? GetUserAgent() => HttpContext.Request.Headers.UserAgent.ToString();
}
