using FluentValidation;
using StockFlow.Application.DTOs;
using StockFlow.Application.Interfaces;

namespace StockFlow.Application.Validators;

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    private readonly IUnitOfWork _uow;

    public CreateProductDtoValidator(IUnitOfWork uow)
    {
        _uow = uow;

        // Performans: Bir alanda hata varsa sonrakine geçmez
        RuleLevelCascadeMode = CascadeMode.Stop;

        // 1. Ürün Adı Kontrolü
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ürün adı boş olamaz.")
            .MinimumLength(2).WithMessage("Ürün adı en az 2 karakter olmalıdır.")
            .MustAsync(async (name, _) => !await _uow.Products.AnyAsync(p => p.Name == name && !p.IsDeleted))
            .WithMessage("Bu ürün adı zaten kullanılıyor.");

        // 2. Barkod Kontrolü (Opsiyonel ama girilirse benzersiz olmalı)
        RuleFor(x => x.Barcode)
            .MinimumLength(3).When(x => !string.IsNullOrEmpty(x.Barcode)).WithMessage("Barkod en az 3 karakter olmalıdır.")
            .MustAsync(async (barcode, _) => 
            {
                if (string.IsNullOrEmpty(barcode)) return true;
                return !await _uow.Products.AnyAsync(p => p.Barcode == barcode && !p.IsDeleted);
            })
            .WithMessage("Bu barkod zaten başka bir ürüne tanımlı.");

        // 3. Kategori Kontrolü
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Lütfen geçerli bir kategori seçin.")
            .MustAsync(async (id, _) => await _uow.Categories.AnyAsync(c => c.Id == id))
            .WithMessage("Seçilen kategori sistemde mevcut değil.");

        // 4. Fiyat ve KDV Kontrolleri
        RuleFor(x => x.PurchasePrice)
            .GreaterThan(0).WithMessage("Alış fiyatı 0'dan büyük olmalıdır.");

        RuleFor(x => x.TaxRate)
            .InclusiveBetween(0, 100).When(x => x.TaxRate.HasValue)
            .WithMessage("KDV oranı %0 ile %100 arasında olmalıdır.");

        // 5. Satış Stratejisi Kontrolü (Fiyat veya Kâr Oranı)
        RuleFor(x => x)
            .Must(x => x.SalePrice.HasValue || x.ProfitRate.HasValue)
            .WithMessage("Ürün için ya bir satış fiyatı ya da kâr oranı girmelisiniz.");

        RuleFor(x => x.SalePrice)
            .GreaterThan(x => x.PurchasePrice)
            .When(x => x.SalePrice.HasValue)
            .WithMessage("Satış fiyatı alış fiyatından düşük olamaz (Zararına satış engeli).");

        // 6. Stok Kontrolü
        RuleFor(x => x.InitialStock)
            .GreaterThanOrEqualTo(0).WithMessage("Başlangıç stoğu negatif olamaz.");
    }
}