using Microsoft.EntityFrameworkCore;
using ZenBudget.Domain.Entities;
using ZenBudget.Domain.Interfaces;
using ZenBudget.Infrastructure.Persistence;

namespace ZenBudget.Infrastructure.Repositories;

public class BudgetRepository : IBudgetRepository
{
    private readonly AppDbContext _context;

    public BudgetRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Budget?> GetByIdAsync(Guid id, Guid userId)
    {
        return await _context.Budgets.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
    }

    public async Task<IEnumerable<Budget>> GetMonthlyBudgetsAsync(Guid userId, int year, int month)
    {
        return await _context.Budgets
            .Include(b => b.Category) // Kategori ismini alabilmek için
            .Where(b => b.UserId == userId && b.Year == year && b.Month == month)
            .ToListAsync();
    }

    public async Task AddAsync(Budget budget)
    {
        await _context.Budgets.AddAsync(budget);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Budget budget)
    {
        _context.Budgets.Update(budget);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var budget = await GetByIdAsync(id, userId);
        if (budget != null)
        {
            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();
        }
    }

    // Aynı aya ve aynı kategoriye ikinci bir bütçe girilmesini engellemek için
    public async Task<bool> ExistsAsync(Guid userId, Guid categoryId, int year, int month)
    {
        return await _context.Budgets.AnyAsync(b =>
            b.UserId == userId && b.CategoryId == categoryId && b.Year == year && b.Month == month);
    }
}