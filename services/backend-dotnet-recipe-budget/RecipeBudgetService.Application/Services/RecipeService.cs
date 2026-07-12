using RecipeBudgetService.Domain.Exceptions;
using RecipeBudgetService.Application.Extensions;
using RecipeBudgetService.Application.Mappers;
using RecipeBudgetService.Application.DTOs;
using RecipeBudgetService.Application.Repositories;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("RecipeBudgetService.Tests")]

namespace RecipeBudgetService.Application.Services;

public class RecipeService(IRecipeRepository repository) : IRecipeService
{
    private readonly IRecipeRepository _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    public async Task<RecipeResponse> CreateAsync(RecipeRequest request, Guid userId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Name);
        GuardExtensions.ThrowIfGuidEmpty(userId);

        var exists = await _repository
            .ExistsByNameAsync(request.Name, userId, cancellationToken);

        if (exists)
        {
            throw new ConflictException($"A recipe with the name '{request.Name}' already exists.");
        }

        var entity = request.ToEntity();
        entity.UserId = userId;

        var created = await _repository.CreateAsync(entity, cancellationToken);
        return created.ToResponse();
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        GuardExtensions.ThrowIfGuidEmpty(id);
        GuardExtensions.ThrowIfGuidEmpty(userId);

        return await _repository.DeleteAsync(id, userId, cancellationToken);
    }

    public async Task<IEnumerable<RecipeResponse>> GetAllAsync(Guid userId, CancellationToken cancellationToken)
    {
        var recipes = await _repository.GetAllAsync(userId, cancellationToken);
        return recipes is null
            ? Enumerable.Empty<RecipeResponse>()
            : recipes.Select(r => r.ToResponse());
    }

    public async Task<RecipeResponse> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        GuardExtensions.ThrowIfGuidEmpty(id);
        GuardExtensions.ThrowIfGuidEmpty(userId);

        var recipe = await _repository.GetByIdAsync(id, userId, cancellationToken);

        return recipe is null
            ? throw new NotFoundException($"Recipe with ID {id} not found.")
            : recipe.ToResponse();
    }

    public async Task<RecipeResponse> UpdateAsync(Guid id, RecipeRequest request, Guid userId, CancellationToken cancellationToken)
    {
        GuardExtensions.ThrowIfGuidEmpty(id);
        GuardExtensions.ThrowIfGuidEmpty(userId);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Name);

        var exists = await _repository.ExistsByNameAsync(request.Name, userId, cancellationToken);
        if (exists)
        {
            throw new ConflictException($"A recipe with the name '{request.Name}' already exists.");
        }

        var updated = await _repository.UpdateAsync(request.ToEntity(id), userId, cancellationToken);
        return updated is null
            ? throw new NotFoundException($"Recipe with ID {id} not found.")
            : updated.ToResponse();
    }
}
