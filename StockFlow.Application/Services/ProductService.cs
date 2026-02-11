using Microsoft.EntityFrameworkCore;
using StockFlow.Application.DTOs;
using StockFlow.Application.Interfaces;
using StockFlow.Domain.Entities; // Entities yazımını düzelttiğini varsayıyorum
using StockFlow.Infrastructure.Persistence;
using System.Globalization; // Kültür desteği için şart
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System; // DateTime.UtcNow özelliğini tanıması için
namespace StockFlow.Application.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public async Task<List<ProductListDto>> GetAllProductsAsync(string? search = null, int? categoryId = null)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Where(p => !p.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(p => 
                p.Name.ToLower().Contains(s) || 
                (p.Barcode != null && p.Barcode.ToLower().Contains(s)) || 
                (p.Description != null && p.Description.ToLower().Contains(s))
            );
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        return await query
            .OrderByDescending(p => p.CreatedDate)
            .Select(p => new ProductListDto 
            {
                Id = p.Id,
                Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(p.Name.ToLower()),
                Barcode = p.Barcode,
                Description = p.Description,
                SalePrice = p.SalePrice,
                StockQuantity = p.StockQuantity,
                CategoryName = p.Category != null ? p.Category.Name : "Kategorisiz",
                CreatedDate = p.CreatedDate
            })
            .ToListAsync();
    }
public async Task<bool> UpdateProductAsync(int id, UpdateProductDto dto)
{
    var product = await _context.Products.FindAsync(id);
    if (product == null || product.IsDeleted) return false;

    // 1. Loglama için eski değerleri tut (Karşılaştırma için önemli)
    var oldValues = new Dictionary<string, string>
    {
        { "Name", product.Name },
        { "Description", product.Description ?? "" },
        { "SalePrice", product.SalePrice.ToString() }
    };

    // 2. STANDARTLAŞTIRMA (Title Case Uygulaması)
    // Önce ToLower ile tüm harfleri küçültüyoruz, sonra ToTitleCase ile her kelimenin ilk harfini büyütüyoruz.
    product.Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dto.Name.ToLower());

    if (!string.IsNullOrEmpty(dto.Description))
    {
        product.Description = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dto.Description.ToLower());
    }

    // 3. Fiyat ve Diğer Güncellemeler
    product.PurchasePrice = dto.PurchasePrice;
    product.UpdatedDate = DateTime.UtcNow;

    if (dto.ProfitRate.HasValue && dto.ProfitRate.Value > 0)
    {
        product.ProfitRate = dto.ProfitRate.Value;
        product.SalePrice = product.PurchasePrice * (1 + (product.ProfitRate / 100));
    }
    else if (dto.SalePrice.HasValue)
    {
        product.SalePrice = dto.SalePrice.Value;
        if (product.PurchasePrice > 0)
        {
            product.ProfitRate = ((product.SalePrice - product.PurchasePrice) / product.PurchasePrice) * 100;
        }
    }

    // 4. LOGLAMA: Formatlanmış yeni isim ile eski ismi karşılaştırıyoruz
    if (oldValues["Name"] != product.Name)
        await AddAuditLog(product.Id, "Name", oldValues["Name"], product.Name);

    if (oldValues["Description"] != product.Description)
        await AddAuditLog(product.Id, "Description", oldValues["Description"], product.Description??"");

    if (oldValues["SalePrice"] != product.SalePrice.ToString())
        await AddAuditLog(product.Id, "SalePrice", oldValues["SalePrice"], product.SalePrice.ToString());

    await _context.SaveChangesAsync();
    return true;
}
    public async Task<List<ProductAuditLogDto>> GetProductHistoryAsync(int productId)
    {
        return await _context.ProductAuditLogs
            .Where(x => x.ProductId == productId)
            .OrderByDescending(x => x.CreatedDate) // En yeni değişiklik en üstte
            .Select(x => new ProductAuditLogDto
            {
                FieldName = x.FieldName,
                OldValue = x.OldValue,
                NewValue = x.NewValue,
                Action = x.Action,
                CreatedAt = x.CreatedDate // Senin BaseEntity'deki ismin CreatedDate idi
            })
            .ToListAsync();
    }

    public Task<bool> UpdateStockAsync(int productId, int quantity)
    {
        throw new NotImplementedException();
    }

    // Log yazma yardımcı metodu
    private async Task AddAuditLog(int productId, string field, string oldVal, string newVal)
    {
        var log = new ProductAuditLog
        {
            ProductId = productId,
            FieldName = field,
            OldValue = oldVal,
            NewValue = newVal,
            Action = "Update",
            CreatedDate = DateTime.UtcNow
        };
        _context.ProductAuditLogs.Add(log);
        await Task.CompletedTask;
    }

    public ProductService(AppDbContext context)
    {
        _context = context;
    }
    
    
    public async Task<int> CreateProductAsync(CreateProductDto dto)
    {
        // 1. Barkod Otomasyonu
        string finalBarcode = string.IsNullOrEmpty(dto.Barcode) 
            ? "SF" + DateTime.Now.Ticks.ToString().Substring(10) // Örn: SF84920
            : dto.Barcode;

        // 2. Fiyat ve Kar Hesaplama (Mevcut mantığın)
        decimal finalSalePrice = 0;
        decimal finalProfitRate = 0;

        if (dto.ProfitRate.HasValue)
        {
            finalProfitRate = dto.ProfitRate.Value;
            finalSalePrice = dto.PurchasePrice * (1 + (finalProfitRate / 100));
        }
        else if (dto.SalePrice.HasValue)
        {
            finalSalePrice = dto.SalePrice.Value;
            finalProfitRate = dto.PurchasePrice > 0 ? ((finalSalePrice - dto.PurchasePrice) / dto.PurchasePrice) * 100 : 0;
        }
       
        
        var product = new Product
        {
            Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dto.Name.ToLower()),
            Barcode = finalBarcode, // Otomatik veya el ile gelen barkod
            Description = !string.IsNullOrEmpty(dto.Description) 
                ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dto.Description.ToLower()) 
                : dto.Description,
            PurchasePrice = dto.PurchasePrice,
            SalePrice = finalSalePrice,
            ProfitRate = finalProfitRate,
            TaxRate = dto.TaxRate ?? 20, // Boşsa %20 yap
            StockQuantity = dto.InitialStock,
            CategoryId = dto.CategoryId,
            CreatedDate = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product.Id;
    }

    
    
    public async Task<List<ProductDto>> GetLowStockProductsAsync()
    {
        var products = await _context.Products
            .Where(p => !p.IsDeleted && p.StockQuantity <= p.CriticalStockLevel) // ?? 5 kısmını sildik
            .ToListAsync();

        return products.Select(p => new ProductDto 
        {
            Id = p.Id,
            Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(p.Name.ToLower()), 
            StockQuantity = p.StockQuantity,
            Barcode = p.Barcode ?? "", // Null ise boş string gönder
            SalePrice = p.SalePrice
        }).ToList(); 
    }
    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return false;

        product.IsDeleted = true; // Ürün hala veritabanında ama listede görünmeyecek
        product.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}