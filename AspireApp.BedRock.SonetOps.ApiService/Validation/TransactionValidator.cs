using FluentValidation;

namespace AspireApp.BedRock.SonetOps.ApiService.Validation;

public class TransactionValidator : AbstractValidator<TransactionRequest>
{
    public TransactionValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0");

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Description is required and must not exceed 100 characters");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3)
            .Matches("^[A-Z]{3}$")
            .WithMessage("Currency must be a valid 3-letter ISO currency code");
    }
}