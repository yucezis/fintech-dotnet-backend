using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ZenBudget.Domain.Interfaces;
using ZenBudget.Infrastructure.Persistence;
using ZenBudget.Infrastructure.Repositories;
using Microsoft.OpenApi.Models;
using ZenBudget.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 1. Controller servislerini ekliyoruz (AuthController'ý tanýmasý için þart)
builder.Services.AddControllers();

// Redis Cache Entegrasyonu
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
    options.InstanceName = "ZenBudget_"; // Cache anahtarlarýnýn baþýna bu ismi ekler
});

var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Key"];

// ?? ESKÝ KODUNU BUNUNLA DEÐÝÞTÝR ??

builder.Services.AddAuthentication(options =>
{
    // .NET'e kapýda her zaman JWT kullanmasýný açýkça söylüyoruz
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuer = true,
        ValidateAudience = true,

        // KRÝTÝK DEÐÝÞÝKLÝK: Saat farký esnekliðini siliyoruz (veya varsayýlan olan 5 dk'ya býrakýyoruz)
        // ClockSkew = TimeSpan.Zero <-- BU SATIRI SÝLDÝK
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            // Bu Visual Studio Output'ta görünür
            System.Diagnostics.Debug.WriteLine($"? JWT HATA: {context.Exception.GetType().Name}: {context.Exception.Message}");
            Console.WriteLine($"? JWT HATA: {context.Exception.GetType().Name}: {context.Exception.Message}");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// 2. Görsel Swagger arayüzü servislerini ekliyoruz
builder.Services.AddEndpointsApiExplorer();
// 2. Görsel Swagger arayüzü servislerini (JWT Destekli) ekliyoruz
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ZenBudget API", Version = "v1" });

    // Swagger'a JWT kullanacaðýmýzý söylüyoruz
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Lütfen giriþ yaptýktan sonra aldýðýnýz uzun Token metnini buraya yapýþtýrýn."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

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

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

// 4. Yazdýðýmýz Controller'larýn rotalarýný sisteme haritalýyoruz
app.MapControllers();
app.Run();