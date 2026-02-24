using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using ZenBudget.Application.DTOs;
using ZenBudget.Domain.Entities;
using ZenBudget.Domain.Interfaces;

namespace ZenBudget.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/reports")]
public class ReportController : ControllerBase
{
    private readonly ITransactionRepository _repository;
    private readonly IDistributedCache _cache; // Redis arayüzümüz

    public ReportController(ITransactionRepository repository, IDistributedCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // 1. Aylık Özet (Dashboard'dakinin hafifletilmiş hali)
    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlyReport([FromQuery] int year, [FromQuery] int month)
    {
        var userId = GetUserId();
        var cacheKey = $"report_monthly_{userId}_{year}_{month}";

        // 1. Adım: Redis'e bak, veri varsa direkt oradan dön! (Sıfır veritabanı sorgusu)
        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedData))
            return Ok(JsonSerializer.Deserialize<MonthlyReportResponse>(cachedData));

        // 2. Adım: Redis'te yoksa, mecburen veritabanından hesapla
        var transactions = await _repository.GetMonthlyTransactionsAsync(userId, year, month);
        var response = new MonthlyReportResponse
        {
            TotalIncome = transactions.Where(t => t.type == TransactionType.Income).Sum(t => t.Amount),
            TotalExpense = transactions.Where(t => t.type == TransactionType.Expense).Sum(t => t.Amount),
            NetBalance = transactions.Where(t => t.type == TransactionType.Income).Sum(t => t.Amount) -
                         transactions.Where(t => t.type == TransactionType.Expense).Sum(t => t.Amount)
        };

        // 3. Adım: Hesaplanan bu veriyi 1 saatliğine Redis'e kaydet
        var cacheOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) };
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response), cacheOptions);

        return Ok(response);
    }

    // 2. Dönemsel Kategori Dağılımı (Pasta grafik için)
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategoryReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var userId = GetUserId();
        // Cache Key'de tarihleri de tutuyoruz ki farklı tarih sorguları birbirine karışmasın
        var cacheKey = $"report_categories_{userId}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";

        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedData))
            return Ok(JsonSerializer.Deserialize<List<CategoryReportResponse>>(cachedData));

        // ITransactionRepository'de tarihe göre getiren bir metoda ihtiyacımız olacak (aşağıda ekleyeceğiz)
        var transactions = await _repository.GetByUserIdAsync(userId); // Şimdilik hepsini çektik
        var filteredTransactions = transactions.Where(t => t.Date >= startDate && t.Date <= endDate && t.type == TransactionType.Expense).ToList();

        var response = filteredTransactions
            .GroupBy(t => new { t.Category?.Name, t.Category?.Color })
            .Select(g => new CategoryReportResponse
            {
                CategoryName = g.Key.Name ?? "Diğer",
                Color = g.Key.Color ?? "#000000",
                TotalAmount = g.Sum(t => t.Amount)
            }).ToList();

        var cacheOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) };
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response), cacheOptions);

        return Ok(response);
    }

    // 3. Son 6 Aylık Trend (Çizgi grafik için)
    [HttpGet("trend")]
    public async Task<IActionResult> GetTrendReport([FromQuery] int months = 6)
    {
        var userId = GetUserId();
        var cacheKey = $"report_trend_{userId}_{months}";

        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedData))
            return Ok(JsonSerializer.Deserialize<List<TrendReportResponse>>(cachedData));

        var transactions = await _repository.GetByUserIdAsync(userId);
        var startDate = DateTime.UtcNow.AddMonths(-months);

        var response = transactions
            .Where(t => t.Date >= startDate)
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .Select(g => new TrendReportResponse
            {
                MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM"), 
                Year = g.Key.Year,
                Income = g.Sum(t => t.type == TransactionType.Income ? t.Amount : 0),
                Expense = g.Sum(t => t.type == TransactionType.Expense ? t.Amount : 0)
            })
            .OrderBy(r => r.Year).ThenBy(r => DateTime.ParseExact(r.MonthName, "MMMM", null).Month)
            .ToList();

        var cacheOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) };
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response), cacheOptions);

        return Ok(response);
    }
}