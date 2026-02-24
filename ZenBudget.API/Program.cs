using Microsoft.EntityFrameworkCore;
using ZenBudget.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// 1. Controller servislerini ekliyoruz (AuthController'ý tanýmasý için þart)
builder.Services.AddControllers();

// 2. Görsel Swagger arayüzü servislerini ekliyoruz
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Veritabaný baðlantýmýz
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// HTTP istek boru hattýný yapýlandýrýyoruz (Pipeline)
if (app.Environment.IsDevelopment())
{
    // 3. Swagger arayüzünü tarayýcýda kullanýma açýyoruz
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 4. Yazdýðýmýz Controller'larýn rotalarýný sisteme haritalýyoruz
app.MapControllers();

app.Run();