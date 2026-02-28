using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZenBudget.Application.DTOs;
using ZenBudget.Domain.Entities;
using ZenBudget.Domain.Interfaces;

namespace ZenBudget.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/budgets")]
public class BudgetController : ControllerBase
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly ITransactionRepository _transactionRepository;

    // Hem Budget hem de Transaction repository'lerini çağırıyoruz!
    public BudgetController(IBudgetRepository budgetRepository, ITransactionRepository transactionRepository)
    {
        _budgetRepository = budgetRepository;
        _transactionRepository = transactionRepository;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetRequest request)
    {
        var userId = GetUserId();

        // İş Kuralı: Bir kategoriye bir ayda sadece 1 bütçe eklenebilir
        if (await _budgetRepository.ExistsAsync(userId, request.CategoryId, request.Year, request.Month))
            return BadRequest("Bu kategori için bu ayda zaten bir bütçe tanımlanmış.");

        var budget = new Budget
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CategoryId = request.CategoryId,
            Amount = request.Amount,
            Month = request.Month,
            Year = request.Year
        };

        await _budgetRepository.AddAsync(budget);
        return Ok(new { message = "Bütçe hedefi başarıyla oluşturuldu.", id = budget.Id });
    }

    [HttpGet]
    public async Task<IActionResult> GetBudgets([FromQuery] int? year, [FromQuery] int? month)
    {
        var userId = GetUserId();
        var targetYear = year ?? DateTime.UtcNow.Year;
        var targetMonth = month ?? DateTime.UtcNow.Month;

        // 1. O ayki bütçe hedeflerini getir
        var budgets = await _budgetRepository.GetMonthlyBudgetsAsync(userId, targetYear, targetMonth);

        // 2. O ayki HARCAMALARI getir (Hesaplama yapmak için)
        var transactions = await _transactionRepository.GetMonthlyTransactionsAsync(userId, targetYear, targetMonth);

        // 3. İkisini eşleştirip "SpentAmount" değerini anlık olarak bul
        var response = budgets.Select(b => new BudgetResponse
        {
            Id = b.Id,
            CategoryId = b.CategoryId,
            CategoryName = b.Category?.Name ?? "Bilinmeyen Kategori",
            TargetAmount = b.Amount,
            Month = b.Month,
            Year = b.Year,
            // Sadece bu kategoriye ait olan "Gider" (Expense) türündeki işlemlerin tutarlarını topla
            SpentAmount = transactions
                .Where(t => t.CategoryId == b.CategoryId && t.type == TransactionType.Expense)
                .Sum(t => t.Amount)
        }).ToList();

        return Ok(response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBudget(Guid id, [FromBody] UpdateBudgetRequest request)
    {
        var budget = await _budgetRepository.GetByIdAsync(id, GetUserId());
        if (budget == null) return NotFound("Bütçe bulunamadı.");

        budget.Amount = request.Amount; // Sadece hedef tutarı güncelliyoruz
        await _budgetRepository.UpdateAsync(budget);

        return Ok(new { message = "Bütçe hedefi başarıyla güncellendi." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBudget(Guid id)
    {
        await _budgetRepository.DeleteAsync(id, GetUserId());
        return Ok(new { message = "Bütçe başarıyla silindi." });
    }
}