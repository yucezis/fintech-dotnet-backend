using ZenBudget.Domain.Entities;

namespace ZenBudget.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetByUserIdAsync(Guid userId);
    Task AddAsync(Category category);
    Task DeleteAsync(Guid id, Guid userId);
    Task<bool> AnyAsync(Guid id);
}