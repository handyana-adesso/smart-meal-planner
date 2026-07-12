using Microsoft.EntityFrameworkCore;
using RecipeBudgetService.Domain.Entities;
using RecipeBudgetService.Application.Repositories;
using RecipeBudgetService.Infrastructure.Data;

namespace RecipeBudgetService.Infrastructure.Repositories;

public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public async Task<User> CreateAsync(User user)
    {
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await dbContext.Users
            .AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == id);
    }
}
