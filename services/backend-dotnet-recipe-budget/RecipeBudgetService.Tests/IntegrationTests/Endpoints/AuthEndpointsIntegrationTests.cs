using FluentAssertions;
using RecipeBudgetService.Application.DTOs.Auth;
using RecipeBudgetService.Domain.Entities;
using System.Net;
using System.Net.Http.Json;

namespace RecipeBudgetService.Tests.IntegrationTests.Endpoints;

public class AuthEndpointsIntegrationTests : BaseEndpointsIntegrationTests
{
    [Fact]
    public async Task POST_ApiAuthRegister_WithValidRequest_ShouldReturn201()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest($"{Guid.NewGuid()}@example.com", "SecurePass1"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task POST_ApiAuthRegister_ShouldReturnAccessAndRefreshTokens()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest($"{Guid.NewGuid()}@example.com", "SecurePass1"));
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();

        // Assert
        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
        body.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task POST_ApiAuthRegister_WhenEmailAlreadyExists_ShouldReturn409()
    {
        // Arrange
        var email = $"{Guid.NewGuid()}@example.com";
        await Client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "SecurePass1"));

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "AnotherPass1"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task POST_ApiAuthRegister_WhenEmailIsInvalid_ShouldReturn400()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("not-an-email", "SecurePass1"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_ApiAuthRegister_WhenPasswordIsTooShort_ShouldReturn400()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest($"{Guid.NewGuid()}@example.com", "Sh0rt"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_ApiAuthRegister_WhenPasswordHasNoUppercase_ShouldReturn400()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest($"{Guid.NewGuid()}@example.com", "nouppercase1"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_ApiAuthRegister_WhenPasswordHasNoNumber_ShouldReturn400()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest($"{Guid.NewGuid()}@example.com", "NoNumberHere"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_ApiAuthLogin_WithValidCredentials_ShouldReturn200()
    {
        // Arrange
        var email = $"{Guid.NewGuid()}@example.com";
        await Client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "SecurePass1"));

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "SecurePass1"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task POST_ApiAuthLogin_WhenEmailDoesNotExist_ShouldReturn401()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest($"{Guid.NewGuid()}@example.com", "SecurePass1"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_ApiAuthLogin_WhenPasswordIsWrong_ShouldReturn401()
    {
        // Arrange
        var email = $"{Guid.NewGuid()}@example.com";
        await Client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "SecurePass1"));

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "WrongPassword1"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_ApiAuthRefresh_WithValidToken_ShouldReturn200WithNewTokens()
    {
        // Arrange
        var email = $"{Guid.NewGuid()}@example.com";
        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "SecurePass1"));
        var registerBody = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(registerBody!.RefreshToken));
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBe(registerBody.RefreshToken);
    }

    [Fact]
    public async Task POST_ApiAuthRefresh_WithRevokedToken_ShouldReturn401()
    {
        // Arrange
        var (token, _) = await SeedRefreshTokenAsync(isRevoked: true);

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(token));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_ApiAuthRefresh_WithExpiredToken_ShouldReturn401()
    {
        // Arrange
        var (token, _) = await SeedRefreshTokenAsync(expiresAt: DateTime.UtcNow.AddDays(-1));

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(token));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_ApiAuthLogout_ShouldReturn204()
    {
        // Arrange
        var email = $"{Guid.NewGuid()}@example.com";
        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "SecurePass1"));
        var registerBody = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/logout", new RefreshTokenRequest(registerBody!.RefreshToken));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task POST_ApiAuthLogout_ShouldInvalidateRefreshToken()
    {
        // Arrange
        var email = $"{Guid.NewGuid()}@example.com";
        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "SecurePass1"));
        var registerBody = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        await Client.PostAsJsonAsync("/api/auth/logout", new RefreshTokenRequest(registerBody!.RefreshToken));

        // Act — try to use the revoked refresh token
        var response = await Client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(registerBody.RefreshToken));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<(string Token, Guid UserId)> SeedRefreshTokenAsync(bool isRevoked = false, DateTime? expiresAt = null)
    {
        return await SeedAsync(async db =>
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = $"{Guid.NewGuid()}@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("SecurePass1")
            };
            db.Users.Add(user);

            var token = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = Guid.NewGuid().ToString(),
                IsRevoked = isRevoked,
                ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(7),
                UserId = user.Id
            };
            db.RefreshTokens.Add(token);

            await db.SaveChangesAsync();
            return (token.Token, user.Id);
        });
    }
}
