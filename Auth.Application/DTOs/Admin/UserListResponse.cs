using Auth.Application.DTOs.User;

namespace Auth.Application.DTOs.Admin;

public class UserListResponse
{
    public List<UserProfileResponse> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
