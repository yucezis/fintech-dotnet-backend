using Microsoft.EntityFrameworkCore;
using ZenBudget.Domain.Entities;

namespace ZenBudget.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
}