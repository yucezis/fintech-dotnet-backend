using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZenBudget.Domain.Entities;

namespace ZenBudget.Domain.Interfaces;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id);
    Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId);
    Task AddAsync(Transaction transaction);
    Task UpdateAsync(Transaction transaction);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<Transaction>> GetMonthlyTransactionsAsync(Guid userId, int year, int month);
}
