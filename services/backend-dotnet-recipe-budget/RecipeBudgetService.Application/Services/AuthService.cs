using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RecipeBudgetService.Domain.Entities;
using RecipeBudgetService.Domain.Exceptions;
using RecipeBudgetService.Application.DTOs.Auth;
using RecipeBudgetService.Application.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RecipeBudgetService.Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IConfiguration configuration) : IAuthService
{
    private readonly IUserRepository _userRepository = userRepository
        ?? throw new ArgumentNullException(nameof(userRepository));
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository
        ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
    private readonly IConfiguration _configuration = configuration
        ?? throw new ArgumentNullException(nameof(configuration));

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var exists = await _userRepository.ExistsByEmailAsync(request.Email);
        if (exists)
        {
            throw new ConflictException($"A user with the email '{request.Email}' already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        var created = await _userRepository.CreateAsync(user);

        return await GenerateAuthResponseAsync(created);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        var existing = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
        if (existing is null || existing.IsRevoked || existing.ExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedException("Refresh token is invalid, revoked, or expired.");
        }

        await _refreshTokenRepository.RevokeAsync(refreshToken);

        var user = await _userRepository.GetByIdAsync(existing.UserId)
            ?? throw new UnauthorizedException("Refresh token is invalid, revoked, or expired.");

        return await GenerateAuthResponseAsync(user);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        await _refreshTokenRepository.RevokeAsync(refreshToken);
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(User user)
    {
        var accessTokenExpiryMinutes = int.TryParse(_configuration["JWT_ACCESS_TOKEN_EXPIRY_MINUTES"], out var minutes) ? minutes : 15;
        var refreshTokenExpiryDays = int.TryParse(_configuration["JWT_REFRESH_TOKEN_EXPIRY_DAYS"], out var days) ? days : 7;

        var expiresAt = DateTime.UtcNow.AddMinutes(accessTokenExpiryMinutes);
        var accessToken = GenerateAccessToken(user, expiresAt);
        var refreshTokenValue = GenerateRefreshTokenValue();

        await _refreshTokenRepository.CreateAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
            IsRevoked = false,
            UserId = user.Id
        });

        return new AuthResponse(accessToken, refreshTokenValue, expiresAt);
    }

    private string GenerateAccessToken(User user, DateTime expiresAt)
    {
        var secret = _configuration["JWT_SECRET"]
            ?? throw new InvalidOperationException("JWT_SECRET is not configured.");
        var issuer = _configuration["JWT_ISSUER"];
        var audience = _configuration["JWT_AUDIENCE"];

        var claims = new[]
        {
            new Claim("sub", user.Id.ToString()),
            new Claim("email", user.Email)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshTokenValue() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}
