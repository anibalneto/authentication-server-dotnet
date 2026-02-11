using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Auth.Application.DTOs.Auth;
using Auth.Application.DTOs.Common;

namespace Auth.Api.Tests;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<(HttpStatusCode StatusCode, AuthResponse? Auth)> RegisterUserAsync(string? email = null)
    {
        var request = new RegisterRequest
        {
            Email = email ?? $"test-{Guid.NewGuid()}@example.com",
            Password = "Password1!",
            FirstName = "John",
            LastName = "Doe"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        if (!response.IsSuccessStatusCode)
            return (response.StatusCode, null);

        var json = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(json, JsonOptions);
        return (response.StatusCode, apiResponse?.Data);
    }

    [Fact]
    public async Task Register_With_Valid_Data_Returns_Created()
    {
        var (statusCode, auth) = await RegisterUserAsync();

        Assert.Equal(HttpStatusCode.Created, statusCode);
        Assert.NotNull(auth);
        Assert.NotEmpty(auth.AccessToken);
        Assert.NotEmpty(auth.RefreshToken);
    }

    [Fact]
    public async Task Register_With_Duplicate_Email_Returns_Conflict()
    {
        var email = $"dup-{Guid.NewGuid()}@example.com";

        var (firstStatus, _) = await RegisterUserAsync(email);
        Assert.Equal(HttpStatusCode.Created, firstStatus);

        var (secondStatus, _) = await RegisterUserAsync(email);
        Assert.Equal(HttpStatusCode.Conflict, secondStatus);
    }

    [Fact]
    public async Task Register_With_Invalid_Email_Returns_BadRequest()
    {
        var request = new RegisterRequest { Email = "not-valid", Password = "Password1!" };
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_With_Weak_Password_Returns_BadRequest()
    {
        var request = new RegisterRequest { Email = "test@example.com", Password = "weak" };
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_With_Valid_Credentials_Returns_Ok()
    {
        var email = $"login-{Guid.NewGuid()}@example.com";
        var password = "Password1!";

        var (regStatus, _) = await RegisterUserAsync(email);
        Assert.Equal(HttpStatusCode.Created, regStatus);

        var loginRequest = new LoginRequest { Email = email, Password = password };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_With_Wrong_Password_Returns_Unauthorized()
    {
        var email = $"wrongpw-{Guid.NewGuid()}@example.com";
        await RegisterUserAsync(email);

        var loginRequest = new LoginRequest { Email = email, Password = "WrongPass1!" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_With_NonExistent_User_Returns_Unauthorized()
    {
        var loginRequest = new LoginRequest { Email = "nobody@example.com", Password = "Password1!" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_Token_Returns_New_Tokens()
    {
        var (_, auth) = await RegisterUserAsync();
        Assert.NotNull(auth);

        var refreshRequest = new RefreshTokenRequest { RefreshToken = auth.RefreshToken };
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Protected_Endpoint_Without_Token_Returns_Unauthorized()
    {
        var response = await _client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Health_Endpoint_Returns_Ok()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
