using Microsoft.EntityFrameworkCore;
using ZenBudget.Domain.Entities;

namespace ZenBudget.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Budget> Budgets => Set<Budget>();

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Transaction>()
            .Property(t => t.Amount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Budget>()
            .Property(b => b.Amount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Category>().HasData(
        new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111101"), Name = "Market", Icon = "??", Color = "#4CAF50", Type = TransactionType.Expense, IsSystem = true, UserId = null },
        new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111102"), Name = "Ulaþým", Icon = "??", Color = "#2196F3", Type = TransactionType.Expense, IsSystem = true, UserId = null },
        new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111103"), Name = "Yemek", Icon = "???", Color = "#FF9800", Type = TransactionType.Expense, IsSystem = true, UserId = null },
        new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111104"), Name = "Faturalar", Icon = "??", Color = "#9C27B0", Type = TransactionType.Expense, IsSystem = true, UserId = null },
        new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111105"), Name = "Saðlýk", Icon = "??", Color = "#F44336", Type = TransactionType.Expense, IsSystem = true, UserId = null },
        new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111106"), Name = "Eðlence", Icon = "??", Color = "#E91E63", Type = TransactionType.Expense, IsSystem = true, UserId = null },
        new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111107"), Name = "Maaþ", Icon = "??", Color = "#4CAF50", Type = TransactionType.Income, IsSystem = true, UserId = null },
        new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111108"), Name = "Freelance", Icon = "??", Color = "#00BCD4", Type = TransactionType.Income, IsSystem = true, UserId = null },
        new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111109"), Name = "Kira Geliri", Icon = "??", Color = "#FF5722", Type = TransactionType.Income, IsSystem = true, UserId = null }
    );
    }
}