using Auth.Application.DTOs.Auth;
using Auth.Application.Validators;
using FluentValidation.TestHelper;

namespace Auth.Application.Tests.Validators;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public void Should_Pass_With_Valid_Request()
    {
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password1!",
            FirstName = "John",
            LastName = "Doe"
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_Email_Is_Empty()
    {
        var request = new RegisterRequest { Email = "", Password = "Password1!" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Fail_When_Email_Is_Invalid()
    {
        var request = new RegisterRequest { Email = "not-an-email", Password = "Password1!" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Fail_When_Password_Is_Empty()
    {
        var request = new RegisterRequest { Email = "test@example.com", Password = "" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Fail_When_Password_Is_Too_Short()
    {
        var request = new RegisterRequest { Email = "test@example.com", Password = "Pa1!" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Fail_When_Password_Has_No_Uppercase()
    {
        var request = new RegisterRequest { Email = "test@example.com", Password = "password1!" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Fail_When_Password_Has_No_Lowercase()
    {
        var request = new RegisterRequest { Email = "test@example.com", Password = "PASSWORD1!" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Fail_When_Password_Has_No_Number()
    {
        var request = new RegisterRequest { Email = "test@example.com", Password = "Password!" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Fail_When_Password_Has_No_Special_Character()
    {
        var request = new RegisterRequest { Email = "test@example.com", Password = "Password1" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
