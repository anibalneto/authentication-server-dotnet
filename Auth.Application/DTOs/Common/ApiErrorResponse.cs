namespace Auth.Application.DTOs.Common;

public class ApiErrorResponse
{
    public bool Success { get; set; }
    public ErrorDetail Error { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiErrorResponse Create(string code, string message, IEnumerable<string>? details = null)
    {
        return new ApiErrorResponse
        {
            Success = false,
            Error = new ErrorDetail
            {
                Code = code,
                Message = message,
                Details = details?.ToList() ?? new List<string>()
            }
        };
    }
}

public class ErrorDetail
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<string> Details { get; set; } = new();
}
