using BWell.Loyalty.Application.DTOs.Auth;
using BWell.Loyalty.Application.DTOs.Programs;
using BWell.Loyalty.Application.DTOs.Subscriptions;
using BWell.Loyalty.Application.DTOs.Visits;
using FluentValidation;

namespace BWell.Loyalty.Application.Validation;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
    }
}

public class StartCheckoutRequestValidator : AbstractValidator<StartCheckoutRequest>
{
    public StartCheckoutRequestValidator()
    {
        RuleFor(x => x.LoyaltyProgramId).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MobileNumber).NotEmpty().MaximumLength(30)
            .Matches(@"^[\d+\s\-()]{8,30}$").WithMessage("Invalid mobile number.");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Age).InclusiveBetween(1, 120).When(x => x.Age.HasValue);
        RuleFor(x => x.NationalIdOrPassport).MaximumLength(50);
        RuleFor(x => x.FamilyMembers!).Must(m => m.Count <= 4)
            .When(x => x.FamilyMembers is not null)
            .WithMessage("Family plans support up to 4 members.");
        RuleForEach(x => x.FamilyMembers).ChildRules(m =>
        {
            m.RuleFor(f => f.FullName).NotEmpty().MaximumLength(200);
        }).When(x => x.FamilyMembers is not null);
    }
}

public class CompletePaymentRequestValidator : AbstractValidator<CompletePaymentRequest>
{
    public CompletePaymentRequestValidator()
    {
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.DeliveryAddress).NotNull();
        RuleFor(x => x.DeliveryAddress.Governorate).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DeliveryAddress.PostalCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.DeliveryAddress.AddressDetails).NotEmpty().MaximumLength(500);
        RuleFor(x => x.DeliveryAddress.Country).MaximumLength(100);
        RuleFor(x => x.PaymentMethod).IsInEnum();
    }
}

public class CreateLoyaltyProgramRequestValidator : AbstractValidator<CreateLoyaltyProgramRequest>
{
    public CreateLoyaltyProgramRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.IndividualPriceEgp).GreaterThanOrEqualTo(0);
        RuleFor(x => x.FamilyPriceEgp).GreaterThanOrEqualTo(x => x.IndividualPriceEgp);
        RuleFor(x => x.Icon).IsInEnum();
        RuleFor(x => x.ColorGradient).IsInEnum();
    }
}

public class UpdateLoyaltyProgramRequestValidator : AbstractValidator<UpdateLoyaltyProgramRequest>
{
    public UpdateLoyaltyProgramRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.IndividualPriceEgp).GreaterThanOrEqualTo(0);
        RuleFor(x => x.FamilyPriceEgp).GreaterThanOrEqualTo(x => x.IndividualPriceEgp);
    }
}

public class RecordVisitRequestValidator : AbstractValidator<RecordVisitRequest>
{
    public RecordVisitRequestValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.MedicalServiceId).NotEmpty();
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

public class ConfirmPaymentRequestValidator : AbstractValidator<ConfirmPaymentRequest>
{
    public ConfirmPaymentRequestValidator()
    {
        RuleFor(x => x.SubscriptionId).NotEmpty();
    }
}
