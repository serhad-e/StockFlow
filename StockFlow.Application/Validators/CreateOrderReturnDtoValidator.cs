using FluentValidation;
using StockFlow.Application.DTOs;
using StockFlow.Application.Interfaces;

namespace StockFlow.Application.Validators;

public class CreateOrderReturnDtoValidator : AbstractValidator<CreateOrderReturnDto>
{
    private readonly IUnitOfWork _uow;

    public CreateOrderReturnDtoValidator(IUnitOfWork uow)
    {
        _uow = uow;

        // Bir alanda hata varsa o alanın alt kurallarına bakmaz, veritabanını yormaz.
        RuleLevelCascadeMode = CascadeMode.Stop;

        // 1. Sipariş Doğrulaması
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("Geçersiz sipariş ID.")
            .MustAsync(async (id, _) => await _uow.Orders.AnyAsync(o => o.Id == id))
            .WithMessage("İade edilmek istenen sipariş sistemde bulunamadı.");

        // 2. Liste Boşluk Kontrolü
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("İade edilecek en az bir ürün seçilmelidir.");

        // 3. Liste Elemanları İçin Temel Doğrulama (ID ve Miktar)
        RuleForEach(x => x.Items).ChildRules(item => {
            item.RuleFor(i => i.ProductId)
                .GreaterThan(0).WithMessage("Ürün seçimi zorunludur.");
            
            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("İade miktarı en az 1 olmalıdır.");
        });

        // 4. Karmaşık İş Mantığı: Miktar Kontrolü (ActiveQuantity üzerinden)
        // RuleForEach().MustAsync kullanarak parent'taki OrderId'ye erişiyoruz.
        RuleForEach(x => x.Items).MustAsync(async (parentDto, returnItem, cancellation) => 
        {
            // Siparişi satırlarıyla birlikte çekiyoruz (Koleksiyon ismi: OrderItems)
            var order = await _uow.Orders.GetByIdAsync(parentDto.OrderId); 
            
            if (order == null || order.OrderItems == null) return false;

            // Siparişte bu ürün var mı?
            var originalOrderItem = order.OrderItems.FirstOrDefault(oi => oi.ProductId == returnItem.ProductId);
            
            if (originalOrderItem == null) return false;

            // Kontrol: İade edilecek miktar, o satırdaki ActiveQuantity'den (Kalan) büyük olamaz.
            return returnItem.Quantity <= originalOrderItem.ActiveQuantity;
        })
        .WithMessage((parentDto, returnItem) => 
            $"Ürün ID: {returnItem.ProductId} için iade talebiniz, satın alınan veya kalan miktarı aşıyor.");
    }
}