using ZenBudget.Domain.Entities;

namespace ZenBudget.Application.DTOs;

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
}