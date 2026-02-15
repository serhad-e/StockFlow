using System.Globalization;
using StockFlow.Application.DTOs;
using StockFlow.Application.Interfaces;
using StockFlow.Domain.Entities;

namespace StockFlow.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _uow;

    public ProductService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<ProductListDto>> GetAllProductsAsync(string? search = null, int? categoryId = null)
    {
        // Repository üzerinden Include (Category) kullanarak çekiyoruz
        var products = await _uow.Products.GetAllAsync(
            filter: p => !p.IsDeleted && 
                         (string.IsNullOrEmpty(search) || 
                          p.Name.ToLower().Contains(search.ToLower()) || 
                          (p.Barcode != null && p.Barcode.ToLower().Contains(search.ToLower()))),
            orderBy: q => q.OrderByDescending(p => p.CreatedDate),
            includes: p => p.Category!
        );

        if (categoryId.HasValue)
        {
            products = products.Where(p => p.CategoryId == categoryId.Value).ToList();
        }

        return products.Select(p => new ProductListDto 
        {
            Id = p.Id,
            Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(p.Name.ToLower()),
            Barcode = p.Barcode,
            Description = p.Description,
            SalePrice = p.SalePrice,
            StockQuantity = p.StockQuantity,
            CategoryName = p.Category != null ? p.Category.Name : "Kategorisiz",
            CreatedDate = p.CreatedDate
        }).ToList();
    }

    public async Task<int> CreateProductAsync(CreateProductDto dto)
    {
        string finalBarcode = string.IsNullOrEmpty(dto.Barcode) 
            ? "SF" + DateTime.Now.Ticks.ToString().Substring(10) 
            : dto.Barcode;

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
            Barcode = finalBarcode,
            Description = !string.IsNullOrEmpty(dto.Description) 
                ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dto.Description.ToLower()) 
                : dto.Description,
            PurchasePrice = dto.PurchasePrice,
            SalePrice = finalSalePrice,
            ProfitRate = finalProfitRate,
            TaxRate = dto.TaxRate ?? 20,
            StockQuantity = dto.InitialStock,
            CategoryId = dto.CategoryId,
            CreatedDate = DateTime.UtcNow
        };

        await _uow.Products.AddAsync(product);
        await _uow.SaveChangesAsync();
        return product.Id;
    }

    public async Task<bool> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        var product = await _uow.Products.GetByIdAsync(id);
        if (product == null || product.IsDeleted) return false;

        var oldValues = new Dictionary<string, string>
        {
            { "Name", product.Name },
            { "Description", product.Description ?? "" },
            { "SalePrice", product.SalePrice.ToString() }
        };

        product.Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dto.Name.ToLower());
        if (!string.IsNullOrEmpty(dto.Description))
            product.Description = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dto.Description.ToLower());

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
                product.ProfitRate = ((product.SalePrice - product.PurchasePrice) / product.PurchasePrice) * 100;
        }

        if (oldValues["Name"] != product.Name)
            await AddAuditLog(product.Id, "Name", oldValues["Name"], product.Name);

        if (oldValues["Description"] != product.Description)
            await AddAuditLog(product.Id, "Description", oldValues["Description"], product.Description ?? "");

        if (oldValues["SalePrice"] != product.SalePrice.ToString())
            await AddAuditLog(product.Id, "SalePrice", oldValues["SalePrice"], product.SalePrice.ToString());

        _uow.Products.Update(product);
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<List<ProductAuditLogDto>> GetProductHistoryAsync(int productId)
    {
        var logs = await _uow.AuditLogs.GetAllAsync(
            filter: x => x.ProductId == productId,
            orderBy: q => q.OrderByDescending(x => x.CreatedDate)
        );

        return logs.Select(x => new ProductAuditLogDto
        {
            FieldName = x.FieldName,
            OldValue = x.OldValue,
            NewValue = x.NewValue,
            Action = x.Action,
            CreatedAt = x.CreatedDate
        }).ToList();
    }

    public async Task<List<ProductDto>> GetLowStockProductsAsync()
    {
        var products = await _uow.Products.GetAllAsync(
            filter: p => !p.IsDeleted && p.StockQuantity <= p.CriticalStockLevel
        );

        return products.Select(p => new ProductDto 
        {
            Id = p.Id,
            Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(p.Name.ToLower()), 
            StockQuantity = p.StockQuantity,
            Barcode = p.Barcode ?? "",
            SalePrice = p.SalePrice
        }).ToList(); 
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _uow.Products.GetByIdAsync(id);
        if (product == null) return false;

        product.IsDeleted = true;
        product.UpdatedDate = DateTime.UtcNow;

        _uow.Products.Update(product);
        await _uow.SaveChangesAsync();
        return true;
    }

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
        await _uow.AuditLogs.AddAsync(log);
    }

    public Task<bool> UpdateStockAsync(int productId, int quantity)
    {
        throw new NotImplementedException();
    }
}