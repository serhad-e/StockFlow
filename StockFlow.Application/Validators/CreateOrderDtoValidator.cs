using FluentValidation;
using StockFlow.Application.DTOs;
using StockFlow.Application.Interfaces;

namespace StockFlow.Application.Validators;

public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    private readonly IUnitOfWork _uow;

    public CreateOrderDtoValidator(IUnitOfWork uow)
    {
        _uow = uow;

        // Bir alanda ilk hata alındığında o alanın alt kurallarını çalıştırmayı durdurur.
        // Bu sayede geçersiz bir ID için boşuna veritabanına "var mı?" sorgusu atılmaz.
        RuleLevelCascadeMode = CascadeMode.Stop;

        // 1. Müşteri Doğrulaması
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Müşteri seçimi zorunludur.")
            .MustAsync(async (id, _) => await _uow.Customers.AnyAsync(c => c.Id == id && !c.IsDeleted))
            .WithMessage("Seçilen müşteri sistemde bulunamadı veya pasif durumda.");

        // 2. Sipariş Notu Kontrolü (Opsiyonel Alan)
        RuleFor(x => x.Note)
            .MaximumLength(500).WithMessage("Sipariş notu en fazla 500 karakter olabilir.");

        // 3. Sipariş Kalemleri Genel Kontrolü
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Sipariş oluşturabilmek için en az bir ürün eklemelisiniz.");

        // 4. Her Bir Sipariş Satırı İçin Detaylı Kontroller
        RuleForEach(x => x.Items).ChildRules(item => {
            
            // Ürün ID Kontrolü
            item.RuleFor(i => i.ProductId)
                .GreaterThan(0).WithMessage("Geçersiz ürün seçimi.")
                .MustAsync(async (id, _) => await _uow.Products.AnyAsync(p => p.Id == id && !p.IsDeleted))
                .WithMessage("Siparişteki ürünlerden biri sistemde bulunamadı.");

            // Miktar Kontrolü
            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Ürün miktarı 0'dan büyük olmalıdır.");

            // KRİTİK İŞ KURALI: Anlık Stok Yeterliliği
            item.RuleFor(i => i)
                .MustAsync(async (orderItem, _) => {
                    var product = await _uow.Products.GetByIdAsync(orderItem.ProductId);
                    // Ürün yoksa zaten yukarıdaki kural hata verecek, burada stok karşılaştırması yapıyoruz
                    return product != null && product.StockQuantity >= orderItem.Quantity;
                })
                .WithMessage((orderItem) => 
                    $"Ürün ID: {orderItem.ProductId} için yetersiz stok! Mevcut stok miktarını aşıyorsunuz.");
        });
    }
}