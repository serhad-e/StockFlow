using FluentValidation;
using StockFlow.Application.DTOs;
public class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Kategori adı boş olamaz.")
            .MinimumLength(2).WithMessage("Kategori adı çok kısa.");
    }
}