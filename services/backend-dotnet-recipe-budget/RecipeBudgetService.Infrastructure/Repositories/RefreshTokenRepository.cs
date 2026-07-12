using Microsoft.EntityFrameworkCore;
using RecipeBudgetService.Domain.Entities;
using RecipeBudgetService.Application.Repositories;
using RecipeBudgetService.Infrastructure.Data;

namespace RecipeBudgetService.Infrastructure.Repositories;

public class RefreshTokenRepository(AppDbContext dbContext) : IRefreshTokenRepository
{
    public async Task<RefreshToken> CreateAsync(RefreshToken token)
    {
        dbContext.RefreshTokens.Add(token);
        await dbContext.SaveChangesAsync();
        return token;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == token);
    }

    public async Task RevokeAsync(string token)
    {
        var existing = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == token);

        if (existing is null)
        {
            return;
        }

        existing.IsRevoked = true;
        await dbContext.SaveChangesAsync();
    }
}
