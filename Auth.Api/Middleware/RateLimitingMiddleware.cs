using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using Auth.Application.DTOs.Common;

namespace Auth.Api.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, RateLimitEntry> FailedAttempts = new();
    private const int MaxAttempts = 5;
    private const int WindowMinutes = 15;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        if (!path.Contains("/api/auth/login"))
        {
            await _next(context);
            return;
        }

        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var key = $"login:{ipAddress}";

        CleanupExpiredEntries();

        if (FailedAttempts.TryGetValue(key, out var entry))
        {
            if (entry.Attempts >= MaxAttempts && entry.WindowStart.AddMinutes(WindowMinutes) > DateTime.UtcNow)
            {
                _logger.LogWarning("Rate limit exceeded for IP: {IpAddress}", ipAddress);
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.ContentType = "application/json";

                var response = ApiErrorResponse.Create(
                    "RATE_LIMIT_EXCEEDED",
                    $"Too many failed login attempts. Please try again after {WindowMinutes} minutes.");

                var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await context.Response.WriteAsync(json);
                return;
            }
        }

        await _next(context);

        if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
        {
            FailedAttempts.AddOrUpdate(key,
                _ => new RateLimitEntry { Attempts = 1, WindowStart = DateTime.UtcNow },
                (_, existing) =>
                {
                    if (existing.WindowStart.AddMinutes(WindowMinutes) <= DateTime.UtcNow)
                    {
                        return new RateLimitEntry { Attempts = 1, WindowStart = DateTime.UtcNow };
                    }
                    existing.Attempts++;
                    return existing;
                });
        }
        else if (context.Response.StatusCode == (int)HttpStatusCode.OK)
        {
            FailedAttempts.TryRemove(key, out _);
        }
    }

    private static void CleanupExpiredEntries()
    {
        var expiredKeys = FailedAttempts
            .Where(kvp => kvp.Value.WindowStart.AddMinutes(WindowMinutes * 2) <= DateTime.UtcNow)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            FailedAttempts.TryRemove(key, out _);
        }
    }

    private class RateLimitEntry
    {
        public int Attempts { get; set; }
        public DateTime WindowStart { get; set; }
    }
}
