using RecipeBudgetService.Common.Extensions;
using RecipeBudgetService.Common.Mappers;
using RecipeBudgetService.DTOs;
using RecipeBudgetService.Repositories;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("RecipeBudgetService.Tests")]

namespace RecipeBudgetService.Services;

internal class RecipeService(IRecipeRepository repository) : IRecipeService
{
    private readonly IRecipeRepository _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    public async Task<RecipeResponse> CreateAsync(RecipeRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<RecipeResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var recipes = await _repository.GetAllAsync(cancellationToken);
        return recipes is null
            ? Enumerable.Empty<RecipeResponse>()
            : recipes.Select(r => r.ToResponse());
    }

    public async Task<RecipeResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        GuardExtensions.ThrowIfGuidEmpty(id);

        var recipe = await _repository.GetByIdAsync(id, cancellationToken);
        return recipe?.ToResponse();
    }

    public Task<RecipeResponse?> UpdateAsync(Guid id, RecipeRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
