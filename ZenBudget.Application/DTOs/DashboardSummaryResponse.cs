namespace ZenBudget.Application.DTOs;

public class DashboardSummaryResponse
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetBalance { get; set; }
    public List<CategoryExpenseSummary> ExpenseByCategory { get; set; } = [];
}

public class CategoryExpenseSummary
{
    public string CategoryName { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}