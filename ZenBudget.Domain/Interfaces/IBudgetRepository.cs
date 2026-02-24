using ZenBudget.Domain.Entities;

namespace ZenBudget.Domain.Interfaces;

public interface IBudgetRepository
{
    Task<Budget?> GetByIdAsync(Guid id, Guid userId);
    Task<IEnumerable<Budget>> GetMonthlyBudgetsAsync(Guid userId, int year, int month);
    Task AddAsync(Budget budget);
    Task UpdateAsync(Budget budget);
    Task DeleteAsync(Guid id, Guid userId);
    Task<bool> ExistsAsync(Guid userId, Guid categoryId, int year, int month);
}