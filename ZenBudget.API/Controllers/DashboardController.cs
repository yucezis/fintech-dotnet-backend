using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZenBudget.Application.DTOs;
using ZenBudget.Domain.Entities;
using ZenBudget.Domain.Interfaces;

namespace ZenBudget.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly ITransactionRepository _repository;

    public DashboardController(ITransactionRepository repository)
    {
        _repository = repository;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] int? year, [FromQuery] int? month)
    {
        // Kullanıcı tarih göndermezse otomatik olarak 'şu anki' ay ve yılı baz alıyoruz
        var targetYear = year ?? DateTime.UtcNow.Year;
        var targetMonth = month ?? DateTime.UtcNow.Month;

        // 1. O aya ait tüm işlemleri kategorileriyle birlikte veritabanından çek
        var transactions = await _repository.GetMonthlyTransactionsAsync(GetUserId(), targetYear, targetMonth);

        // 2. Matematiksel hesaplamaları yap
        var totalIncome = transactions.Where(t => t.type == TransactionType.Income).Sum(t => t.Amount);
        var totalExpense = transactions.Where(t => t.type == TransactionType.Expense).Sum(t => t.Amount);
        var netBalance = totalIncome - totalExpense;

        // 3. Giderleri kategoriye göre grupla ve toplamlarını al (Pasta grafiği için)
        var expenseByCategory = transactions
            .Where(t => t.type == TransactionType.Expense && t.Category != null)
            .GroupBy(t => new { t.Category!.Name, t.Category.Color, t.Category.Icon })
            .Select(g => new CategoryExpenseSummary
            {
                CategoryName = g.Key.Name,
                Color = g.Key.Color,
                Icon = g.Key.Icon,
                TotalAmount = g.Sum(t => t.Amount) // O kategorideki toplam harcama
            })
            .OrderByDescending(x => x.TotalAmount) // En çok harcananı en üste koy
            .ToList();

        // 4. Flutter'ın beklediği o tek ve muazzam DTO'yu oluşturup gönder
        var response = new DashboardSummaryResponse
        {
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            NetBalance = netBalance,
            ExpenseByCategory = expenseByCategory
        };

        return Ok(response);
    }
}