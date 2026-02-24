namespace ZenBudget.Application.DTOs;

public class CreateBudgetRequest
{
    public Guid CategoryId { get; set; }
    public decimal Amount { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
}

public class UpdateBudgetRequest
{
    public decimal Amount { get; set; }
}

// Dışarıya döneceğimiz veri (Gerçekleşen harcama burada hesaplanıp gönderilecek)
public class BudgetResponse
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal SpentAmount { get; set; } // Veritabanında yok, biz anlık hesaplayacağız!
    public int Month { get; set; }
    public int Year { get; set; }
}