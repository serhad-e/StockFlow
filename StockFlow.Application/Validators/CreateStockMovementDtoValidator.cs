using FluentValidation;
using StockFlow.Application.DTOs;
using StockFlow.Application.Interfaces;

namespace StockFlow.Application.Validators;

public class CreateStockMovementDtoValidator : AbstractValidator<CreateStockMovementDto>
{
    private readonly IUnitOfWork _uow;

    public CreateStockMovementDtoValidator(IUnitOfWork uow)
    {
        _uow = uow;

        // Performans için ilk hatada durur
        RuleLevelCascadeMode = CascadeMode.Stop;

        // 1. Ürün Kontrolü: Sadece ID yetmez, veritabanında var mı?
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Ürün seçimi zorunludur.")
            .MustAsync(async (id, _) => await _uow.Products.AnyAsync(p => p.Id == id && !p.IsDeleted))
            .WithMessage("Seçilen ürün sistemde bulunamadı.");

        // 2. Hareket Tipi Kontrolü
        // (Genelde 1: Giriş, 2: Çıkış olarak kurgulanır)
        RuleFor(x => x.Type)
            .InclusiveBetween(1, 2).WithMessage("Geçersiz hareket tipi. (1: Giriş, 2: Çıkış)");

        // 3. Miktar Kontrolü
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Hareket miktarı 0'dan büyük olmalıdır.");

        // 4. KRİTİK KONTROL: Stok Yeterliliği (Sadece Çıkış İşlemlerinde)
        // Eğer hareket tipi 'Çıkış' (2) ise, mevcut stoktan fazla çıkış yapılamaz.
        RuleFor(x => x)
            .MustAsync(async (dto, _) => 
            {
                // Eğer bu bir giriş (1) işlemiyse stok kontrolüne gerek yok
                if (dto.Type == 1) return true;

                var product = await _uow.Products.GetByIdAsync(dto.ProductId);
                if (product == null) return false;

                // Mevcut stok çıkılmak istenen miktarı karşılıyor mu?
                return product.StockQuantity >= dto.Quantity;
            })
            .WithMessage((dto) => $"Yetersiz stok! Bu üründen en fazla {dto.Quantity} birim çıkış yapabilirsiniz.")
            .When(x => x.Type == 2); // Sadece Çıkış (2) tipinde çalışır

        // 5. Tarih Kontrolü (Opsiyonel)
        // Eğer DTO'da tarih varsa, geleceğe dönük stok hareketi girilmesini engelleyebilirsin.
    }
}