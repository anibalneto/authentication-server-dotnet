using Auth.Infrastructure.Security;

namespace Auth.Application.Tests.Services;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void HashPassword_Returns_Non_Empty_Hash()
    {
        var hash = _hasher.HashPassword("Password1!");
        Assert.NotEmpty(hash);
        Assert.NotEqual("Password1!", hash);
    }

    [Fact]
    public void HashPassword_Returns_BCrypt_Format()
    {
        var hash = _hasher.HashPassword("Password1!");
        Assert.StartsWith("$2", hash);
    }

    [Fact]
    public void VerifyPassword_Returns_True_For_Correct_Password()
    {
        var hash = _hasher.HashPassword("Password1!");
        var result = _hasher.VerifyPassword("Password1!", hash);
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_Returns_False_For_Incorrect_Password()
    {
        var hash = _hasher.HashPassword("Password1!");
        var result = _hasher.VerifyPassword("WrongPassword!", hash);
        Assert.False(result);
    }

    [Fact]
    public void HashPassword_Generates_Different_Hashes_For_Same_Password()
    {
        var hash1 = _hasher.HashPassword("Password1!");
        var hash2 = _hasher.HashPassword("Password1!");
        Assert.NotEqual(hash1, hash2);
    }
}
