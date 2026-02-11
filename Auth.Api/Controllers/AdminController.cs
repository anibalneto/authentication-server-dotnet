using Auth.Application.DTOs.Admin;
using Auth.Application.DTOs.Common;
using Auth.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Api.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly UserService _userService;

    public AdminController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet("users")]
    public async Task<IActionResult> ListUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var result = await _userService.ListUsersAsync(page, pageSize);
        return Ok(ApiResponse<UserListResponse>.SuccessResponse(result));
    }

    [HttpGet("users/{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        if (result == null)
        {
            return NotFound(ApiErrorResponse.Create("USER_NOT_FOUND", "User not found"));
        }

        return Ok(ApiResponse<UserDetailResponse>.SuccessResponse(result));
    }

    [HttpPost("users/{id:guid}/roles")]
    public async Task<IActionResult> AssignRole(Guid id, [FromBody] AssignRoleRequest request)
    {
        var result = await _userService.AssignRoleAsync(id, request.RoleName);
        if (!result)
        {
            return BadRequest(ApiErrorResponse.Create("ASSIGN_ROLE_FAILED", "Failed to assign role. User or role not found, or role already assigned."));
        }

        return Ok(ApiResponse.SuccessResponse("Role assigned successfully"));
    }
}
