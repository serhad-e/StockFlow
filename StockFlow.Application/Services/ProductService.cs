using Microsoft.EntityFrameworkCore;
using StockFlow.Application.DTOs;
using StockFlow.Application.Interfaces;
using StockFlow.Domain.Entities; // Entities yazımını düzelttiğini varsayıyorum
using StockFlow.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System; // DateTime.UtcNow özelliğini tanıması için
namespace StockFlow.Application.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public async Task<bool> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return false;

        // Loglama hazırlığı: Değişmeden önceki hallerini saklayalım
        var oldValues = new Dictionary<string, string>
        {
            { "Name", product.Name },
            { "SalePrice", product.SalePrice.ToString() },
            { "PurchasePrice", product.PurchasePrice.ToString() }
        };

        // Güncelleme işlemi
        product.Name = dto.Name;
        product.PurchasePrice = dto.PurchasePrice;
        product.UpdatedDate = DateTime.UtcNow; // BaseEntity'den gelen alan

        // Fiyat hesaplama mantığı (senin istediğin esnek yapı)
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

        // LOGLAMA: Nelerin değiştiğini kontrol edip log tablosuna yazalım
        if (oldValues["Name"] != product.Name)
            await AddAuditLog(product.Id, "Name", oldValues["Name"], product.Name);
    
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
            Name = dto.Name,
            Barcode = finalBarcode, // Otomatik veya el ile gelen barkod
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

    public async Task<List<ProductDto>> GetAllProductsAsync()
    {
        return await _context.Products
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Barcode = p.Barcode,
                SalePrice = p.SalePrice,
                StockQuantity = p.StockQuantity
            }).ToListAsync();
    }
    public async Task<List<ProductDto>> GetLowStockProductsAsync()
    {
        return await _context.Products
            .Where(p => !p.IsDeleted && p.StockQuantity <= p.CriticalStockLevel)
            .Select(p => new ProductDto {
                Id = p.Id,
                Name = p.Name,
                StockQuantity = p.StockQuantity,
                Barcode = p.Barcode
            }).ToListAsync();
    }
}