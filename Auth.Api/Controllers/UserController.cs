using System.Security.Claims;
using Auth.Application.DTOs.Common;
using Auth.Application.DTOs.User;
using Auth.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;

    public UserController(UserService userService, IValidator<ChangePasswordRequest> changePasswordValidator)
    {
        _userService = userService;
        _changePasswordValidator = changePasswordValidator;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _userService.GetProfileAsync(userId.Value);
        if (result == null) return NotFound();

        return Ok(ApiResponse<UserProfileResponse>.SuccessResponse(result));
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _userService.UpdateProfileAsync(userId.Value, request);
        if (result == null) return NotFound();

        return Ok(ApiResponse<UserProfileResponse>.SuccessResponse(result, "Profile updated successfully"));
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var validation = await _changePasswordValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return BadRequest(ApiErrorResponse.Create("VALIDATION_ERROR", "Validation failed",
                validation.Errors.Select(e => e.ErrorMessage)));
        }

        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

        var result = await _userService.ChangePasswordAsync(userId.Value, request, ipAddress, userAgent);
        if (!result)
        {
            return BadRequest(ApiErrorResponse.Create("INVALID_PASSWORD", "Current password is incorrect"));
        }

        return Ok(ApiResponse.SuccessResponse("Password changed successfully"));
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
}
