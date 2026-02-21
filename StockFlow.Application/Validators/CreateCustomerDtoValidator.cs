using FluentValidation;
using StockFlow.Application.DTOs;
using StockFlow.Application.Interfaces; // ICustomerService veya IUnitOfWork için

public class CreateCustomerDtoValidator : AbstractValidator<CreateCustomerDto>
{
    private readonly ICustomerService _customerService;

    public CreateCustomerDtoValidator(ICustomerService customerService)
    {
        _customerService = customerService;

        // İsim Kontrolü
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Müşteri adı boş olamaz.")
            .MinimumLength(3).WithMessage("Müşteri adı en az 3 karakter olmalıdır.");

        // E-posta Kontrolü (Format + Benzersizlik)
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi boş olamaz.")
            .EmailAddress().WithMessage("Geçersiz e-posta formatı.")
            .MustAsync(async (email, cancellation) => 
                await BeUniqueEmail(email))
            .WithMessage("Bu e-posta adresi zaten sisteme kayıtlı.");

        // Telefon Kontrolü (Format + Benzersizlik)
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Telefon numarası boş olamaz.")
            .Matches(@"^[0-9]+$").WithMessage("Telefon numarası sadece rakamlardan oluşmalıdır.")
            .Length(10, 15).WithMessage("Telefon numarası 10-15 hane arasında olmalıdır.")
            .MustAsync(async (phone, cancellation) => 
                await BeUniquePhone(phone))
            .WithMessage("Bu telefon numarası zaten başka bir müşteriye tanımlı.");

        // Adres Kontrolü
        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Adres alanı boş olamaz.")
            .MinimumLength(10).WithMessage("Lütfen daha detaylı bir adres giriniz.");
    }

    // Veritabanı kontrolü için yardımcı metotlar
    private async Task<bool> BeUniqueEmail(string email)
    {
        // CustomerService veya Repository üzerinden veritabanında bu mail var mı kontrol et
        // Varsa false döner ve validasyon hatası tetiklenir
        return !await _customerService.AnyEmailAsync(email);
    }

    private async Task<bool> BeUniquePhone(string phone)
    {
        return !await _customerService.AnyPhoneAsync(phone);
    }
}