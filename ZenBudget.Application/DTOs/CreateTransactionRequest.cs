using ZenBudget.Domain.Entities;

namespace ZenBudget.Application.DTOs;

public class CreateTransactionRequest
{
    public Guid CategoryId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TransactionType Type { get; set; } 
}