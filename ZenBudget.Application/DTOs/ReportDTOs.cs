namespace ZenBudget.Application.DTOs;

public class MonthlyReportResponse
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetBalance { get; set; }
}

public class CategoryReportResponse
{
    public string CategoryName { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}

public class TrendReportResponse
{
    public string MonthName { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
}