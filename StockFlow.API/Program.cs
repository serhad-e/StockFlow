using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Interfaces;
using StockFlow.Application.Services;
using StockFlow.Infrastructure.Persistence;
using FluentValidation;
using StockFlow.Application.Validators;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı Bağlantısı
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Dependency Injection (DI) Kayıtları
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IStockMovementService, StockMovementService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ICustomerMovementService, CustomerMovementService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// 3. Modern FluentValidation Yapılandırması (Yeni Standart)
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>(); // Validator'ları kaydeder
builder.Services.AddFluentValidationAutoValidation(); // Otomatik validasyonu aktif eder
builder.Services.AddFluentValidationClientsideAdapters(); // İstemci tarafı desteği (isteğe bağlı)

// 4. Controller ve Hata Mesajı Özelleştirmesi
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            return new BadRequestObjectResult(new { 
                message = "Validasyon hataları oluştu.", 
                errors = errors 
            });
        };
    });

// 5. Swagger ve API Araçları
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 6. Middleware Pipeline (İstek Hattı)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();