using FluentValidation;
using StockFlow.Application.DTOs;
using StockFlow.Application.Interfaces;

namespace StockFlow.Application.Validators;

public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
{
    private readonly IUnitOfWork _uow;

    public UpdateProductDtoValidator(IUnitOfWork uow)
    {
        _uow = uow;

        // Bir hata alındığında o alan için diğer kontrollere geçmeyi durdurur (Performans ve çakışan hata mesajlarını önlemek için)
        RuleLevelCascadeMode = CascadeMode.Stop;

        // 1. Ürün Adı Kontrolü
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ürün adı boş olamaz.")
            .MinimumLength(2).WithMessage("Ürün adı en az 2 karakter olmalıdır.");

        // 2. Barkod Kontrolü
        RuleFor(x => x.Barcode)
            .NotEmpty().WithMessage("Barkod alanı boş olamaz.")
            .MinimumLength(3).WithMessage("Barkod en az 3 karakter olmalıdır.");

        // 3. Fiyat Kontrolleri
        RuleFor(x => x.PurchasePrice)
            .GreaterThan(0).WithMessage("Alış fiyatı 0'dan büyük olmalıdır.");

        // Satış Fiyatı veya Kâr Oranı Mantığı
        RuleFor(x => x)
            .Must(x => x.SalePrice.HasValue || x.ProfitRate.HasValue)
            .WithMessage("Ürün için ya bir satış fiyatı ya da kâr oranı belirlemelisiniz.");

        RuleFor(x => x.SalePrice)
            .GreaterThan(x => x.PurchasePrice)
            .When(x => x.SalePrice.HasValue)
            .WithMessage("Satış fiyatı alış fiyatından düşük olamaz.");

        // 4. Kritik Stok Seviyesi Kontrolü
        RuleFor(x => x.CriticalStockLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Kritik stok seviyesi negatif olamaz.");

        // 5. Kâr Oranı Sınırı
        RuleFor(x => x.ProfitRate)
            .InclusiveBetween(1, 1000)
            .When(x => x.ProfitRate.HasValue)
            .WithMessage("Kâr oranı %1 ile %1000 arasında olmalıdır.");
    }
}