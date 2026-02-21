using StockFlow.Application.DTOs;
using StockFlow.Application.Interfaces;
using StockFlow.Domain.Entities;

namespace StockFlow.Application.Services;

public class StockMovementService : IStockMovementService
{
    private readonly IUnitOfWork _uow;

    public StockMovementService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> CreateMovementAsync(CreateStockMovementDto dto)
    {
        // 1. ADIM: ÜRÜN VAR MI SORGULA (Hayati Kontrol)
        var product = await _uow.Products.GetByIdAsync(dto.ProductId);

        // Eğer ürün yoksa veya silinmişse işlemi durdur ve 'false' dön
        if (product == null || product.IsDeleted) 
        {
            return false; 
        }

        // 2. ADIM: STOK YETERLİ Mİ? (Çıkış İşlemleri İçin)
        if (dto.Type == (int)MovementType.Out && product.StockQuantity < dto.Quantity)
        {
            return false; // Stok yetersizse de işlemi iptal et
        }

        // 3. Stok hareketini oluştur
        var movement = new StockMovement
        {
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            Type = (MovementType)dto.Type,
            Reason = dto.Reason,
            MovementDate = DateTime.UtcNow
        };

        // 4. Ürünün ana stok miktarını güncelle
        if (movement.Type == MovementType.In)
            product.StockQuantity += dto.Quantity;
        else
            product.StockQuantity -= dto.Quantity;

        // 5. Değişiklikleri kaydet (İşte Unit of Work'ün gücü: İki işlem tek seferde!)
        await _uow.StockMovements.AddAsync(movement);
        _uow.Products.Update(product);
        
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<List<StockMovementListDto>> GetProductMovementsAsync(int productId)
    {
        var movements = await _uow.StockMovements.GetAllAsync(
            filter: x => x.ProductId == productId,
            orderBy: q => q.OrderByDescending(x => x.MovementDate),
            includes: x => x.Product
        );

        return movements.Select(x => new StockMovementListDto
        {
            Id = x.Id,
            ProductName = x.Product.Name,
            Quantity = x.Quantity,
            Type = x.Type.ToString(),
            Reason = x.Reason,
            MovementDate = x.MovementDate
        }).ToList();
    }
}