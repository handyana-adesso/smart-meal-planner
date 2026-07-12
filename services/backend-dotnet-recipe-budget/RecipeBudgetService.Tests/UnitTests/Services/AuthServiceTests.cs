using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using RecipeBudgetService.Domain.Entities;
using RecipeBudgetService.Domain.Exceptions;
using RecipeBudgetService.Application.DTOs.Auth;
using RecipeBudgetService.Application.Repositories;
using RecipeBudgetService.Application.Services;

namespace RecipeBudgetService.Tests.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JWT_SECRET"] = "unit-test-secret-key-minimum-32-characters-long",
                ["JWT_ISSUER"] = "smart-meal-planner-tests",
                ["JWT_AUDIENCE"] = "smart-meal-planner-client-tests",
                ["JWT_ACCESS_TOKEN_EXPIRY_MINUTES"] = "15",
                ["JWT_REFRESH_TOKEN_EXPIRY_DAYS"] = "7"
            })
            .Build();

        _authService = new AuthService(
            _userRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            configuration);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var request = new RegisterRequest("user@example.com", "SecurePass1");
        _userRepositoryMock
            .Setup(repo => repo.ExistsByEmailAsync(request.Email))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _authService.RegisterAsync(request);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailIsValid_ShouldReturnAuthResponse()
    {
        // Arrange
        var request = new RegisterRequest("user@example.com", "SecurePass1");
        _userRepositoryMock
            .Setup(repo => repo.ExistsByEmailAsync(request.Email))
            .ReturnsAsync(false);
        _userRepositoryMock
            .Setup(repo => repo.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) => user);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task RegisterAsync_ShouldHashPassword_NeverStoreRawPassword()
    {
        // Arrange
        var request = new RegisterRequest("user@example.com", "SecurePass1");
        User? createdUser = null;
        _userRepositoryMock
            .Setup(repo => repo.ExistsByEmailAsync(request.Email))
            .ReturnsAsync(false);
        _userRepositoryMock
            .Setup(repo => repo.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) =>
            {
                createdUser = user;
                return user;
            });

        // Act
        await _authService.RegisterAsync(request);

        // Assert
        createdUser.Should().NotBeNull();
        createdUser!.PasswordHash.Should().NotBe(request.Password);
        BCrypt.Net.BCrypt.Verify(request.Password, createdUser.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task LoginAsync_WhenEmailDoesNotExist_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var request = new LoginRequest("missing@example.com", "SecurePass1");
        _userRepositoryMock
            .Setup(repo => repo.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _authService.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsWrong_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var request = new LoginRequest("user@example.com", "WrongPassword1");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("SecurePass1")
        };
        _userRepositoryMock
            .Setup(repo => repo.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

        // Act
        Func<Task> act = async () => await _authService.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task LoginAsync_WhenCredentialsAreValid_ShouldReturnAuthResponse()
    {
        // Arrange
        var request = new LoginRequest("user@example.com", "SecurePass1");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };
        _userRepositoryMock
            .Setup(repo => repo.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task RefreshAsync_WhenTokenIsRevoked_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "revoked-token",
            IsRevoked = true,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            UserId = Guid.NewGuid()
        };
        _refreshTokenRepositoryMock
            .Setup(repo => repo.GetByTokenAsync(token.Token))
            .ReturnsAsync(token);

        // Act
        Func<Task> act = async () => await _authService.RefreshAsync(token.Token);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task RefreshAsync_WhenTokenIsExpired_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "expired-token",
            IsRevoked = false,
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            UserId = Guid.NewGuid()
        };
        _refreshTokenRepositoryMock
            .Setup(repo => repo.GetByTokenAsync(token.Token))
            .ReturnsAsync(token);

        // Act
        Func<Task> act = async () => await _authService.RefreshAsync(token.Token);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task RefreshAsync_WhenTokenIsValid_ShouldReturnNewAuthResponse()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "user@example.com", PasswordHash = "hash" };
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "valid-token",
            IsRevoked = false,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            UserId = user.Id
        };
        _refreshTokenRepositoryMock
            .Setup(repo => repo.GetByTokenAsync(token.Token))
            .ReturnsAsync(token);
        _userRepositoryMock
            .Setup(repo => repo.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.RefreshAsync(token.Token);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBe(token.Token);
        _refreshTokenRepositoryMock.Verify(repo => repo.RevokeAsync(token.Token), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_ShouldRevokeRefreshToken()
    {
        // Arrange
        const string refreshToken = "some-refresh-token";

        // Act
        await _authService.LogoutAsync(refreshToken);

        // Assert
        _refreshTokenRepositoryMock.Verify(repo => repo.RevokeAsync(refreshToken), Times.Once);
    }
}
