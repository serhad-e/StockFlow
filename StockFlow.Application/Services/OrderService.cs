using StockFlow.Application.DTOs;
using StockFlow.Application.Interfaces;
using StockFlow.Domain.Entities;

namespace StockFlow.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _uow;

    public OrderService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> CreateOrderAsync(CreateOrderDto dto)
    {
        // 1. Müşteriyi Kontrol Et
        var customer = await _uow.Customers.GetByIdAsync(dto.CustomerId);
        if (customer == null || customer.IsDeleted) return false;

        // 2. Ana Sipariş Nesnesini Hazırla
        var order = new Order
        {
            CustomerId = dto.CustomerId,
            OrderDate = DateTime.UtcNow,
            OrderNumber = $"ORD-{DateTime.Now.ToString("yyyyMMdd")}-{Guid.NewGuid().ToString()[..4].ToUpper()}",
            Note = dto.Note,
            TotalAmount = 0
        };

        foreach (var itemDto in dto.Items)
        {
            // 3. Ürünü ve Stok Durumunu Kontrol Et
            var product = await _uow.Products.GetByIdAsync(itemDto.ProductId);
            if (product == null || product.StockQuantity < itemDto.Quantity)
                throw new Exception($"{product?.Name ?? "Ürün"} için stok yetersiz!");

            // 4. Stok Düşüşü Yap
            product.StockQuantity -= itemDto.Quantity;
            _uow.Products.Update(product);

            // 5. Sipariş Kalemini Oluştur (Senin Entity yapına göre)
            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                Quantity = itemDto.Quantity,
                UnitPrice = product.SalePrice,
                TaxRate = 20 // Varsayılan KDV %20
            };

            order.OrderItems.Add(orderItem);
            order.TotalAmount += orderItem.TotalPrice; // Burada senin yazdığın => Quantity * UnitPrice çalışacak

            // 6. Stok Hareket Kaydı (Raporlama için)
            await _uow.StockMovements.AddAsync(new StockMovement
            {
                ProductId = product.Id,
                Quantity = itemDto.Quantity,
                // DEĞİŞİKLİK BURADA: StockMovementType yerine MovementType kullanıyoruz
                Type = MovementType.Out,
                Reason = $"Satış: {order.OrderNumber}",
                MovementDate = DateTime.UtcNow
            });
        }

        // 7. CARİ ENTEGRASYON: Müşteriyi Borçlandır
        customer.Balance += order.TotalAmount;
        await _uow.CustomerMovements.AddAsync(new CustomerMovement
        {
            CustomerId = customer.Id,
            Amount = order.TotalAmount,
            Type = CustomerMovementType.Debt, // Borç
            IsPaid = false, // Satış anında ödeme alınmadı varsayıyoruz (Veresiye Satış)
            Description = $"{order.OrderNumber} nolu sipariş ile borçlandırıldı.",
            OperationDate = DateTime.UtcNow
        });

        // 8. Kaydet
        await _uow.Orders.AddAsync(order);
        await _uow.SaveChangesAsync(); // Hata olursa hiçbir işlem yapılmaz (Transaction)

        return true;
    }

    public async Task<bool> CreateReturnAsync(CreateOrderReturnDto dto)
    {
        // 1. Siparişi kalemleriyle beraber getir
        var order = await _uow.Orders.GetByIdWithItemsAsync(dto.OrderId);
        if (order == null) throw new Exception("İade yapılacak sipariş kaydı bulunamadı!");

        var customer = await _uow.Customers.GetByIdAsync(order.CustomerId);
        decimal totalReturnAmount = 0;

        foreach (var returnItem in dto.Items)
        {
            
            // 2. Ürün bu siparişte var mı?
            var originalLine = order.OrderItems.FirstOrDefault(x => x.ProductId == returnItem.ProductId);
            if (originalLine == null)
                throw new Exception($"Bu siparişte {returnItem.ProductId} Id'li bir ürün satın alınmamış!");

            // 3. KRİTİK KONTROL: Kalan iade hakkını hesapla
            int availableToReturn = originalLine.Quantity - originalLine.ReturnedQuantity;

            if (returnItem.Quantity > availableToReturn)
            {
                throw new Exception($"{originalLine.Product.Name} için kalan iade hakkı {availableToReturn} adet. " +
                                    $"Siz {returnItem.Quantity} adet girmeye çalışıyorsunuz!");
            }

            // --- Kontroller Geçildi, İşlemler Başlasın ---

            // 4. OrderItem'daki iade sayacını güncelle
            originalLine.ReturnedQuantity += returnItem.Quantity;

            // 5. Stokları Geri Yükle (+)
            var product = await _uow.Products.GetByIdAsync(returnItem.ProductId);
            if (product == null) throw new Exception("İşlem yapılmak istenen ürün sistemde bulunamadı.");
            product.StockQuantity += returnItem.Quantity;
            _uow.Products.Update(product);

            // 6. İade Tutarı (Satış anındaki fiyattan)
            totalReturnAmount += (originalLine.UnitPrice * returnItem.Quantity);

            // 7. Stok Hareketi
            await _uow.StockMovements.AddAsync(new StockMovement
            {
                ProductId = product.Id,
                Quantity = returnItem.Quantity,
                Type = MovementType.Return, // Return = 3
                Reason = $"İade: {order.OrderNumber} faturasına istinaden.",
                MovementDate = DateTime.UtcNow
            });
        }
        //customer nesnesinin boş  dönmemesi
        
        if (customer == null)
        {
            // Eğer müşteri yoksa iade işlemini devam ettiremezsin
            throw new Exception("Müşteri bulunamadığı için iade işlemi gerçekleştirilemiyor.");
        }
        // 8. Cari Bakiyeyi Düşür (-)
        customer.Balance -= totalReturnAmount;
        
        
        // 9. Cari Hareket (CustomerMovement)
        await _uow.CustomerMovements.AddAsync(new CustomerMovement
        {
            CustomerId = customer.Id,
            Amount = totalReturnAmount,
            Type = CustomerMovementType.Return, // Return = 3
            IsPaid = true,
            PaymentMethod = PaymentMethod.Cash,
            Description = $"{order.OrderNumber} nolu siparişin iadesi işlendi.",
            OperationDate = DateTime.UtcNow
        });

        await _uow.SaveChangesAsync();
        return true;
    }
}