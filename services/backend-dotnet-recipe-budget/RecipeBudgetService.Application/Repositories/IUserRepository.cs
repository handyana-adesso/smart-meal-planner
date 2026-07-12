using RecipeBudgetService.Domain.Entities;

namespace RecipeBudgetService.Application.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task<User> CreateAsync(User user);
    Task<bool> ExistsByEmailAsync(string email);
}
