using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZenBudget.Application.DTOs;
using ZenBudget.Domain.Entities;
using ZenBudget.Domain.Interfaces;

namespace ZenBudget.API.Controllers;

[Authorize] 
[ApiController]
[Route("api/v1/transactions")]
public class TransactionController : ControllerBase
{
    private readonly ITransactionRepository _repository;

    public TransactionController(ITransactionRepository repository)
    {
        _repository = repository;
    }

    private Guid GetUserId()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userIdString!);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
    {
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = GetUserId(), 
            CategoryId = request.CategoryId,
            Amount = request.Amount,
            Description = request.Description,
            Date = request.Date,
            type = request.Type
        };

        await _repository.AddAsync(transaction);
        return Ok(new { message = "İşlem başarıyla eklendi.", transactionId = transaction.Id });
    }

    [HttpGet]
    public async Task<IActionResult> GetMyTransactions()
    {
        var userId = GetUserId(); 
        var transactions = await _repository.GetByUserIdAsync(userId);
        return Ok(transactions);
    }
}