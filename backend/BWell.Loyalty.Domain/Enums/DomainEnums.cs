namespace BWell.Loyalty.Domain.Enums;

public enum PlanType
{
    Individual = 1,
    Family = 2
}

public enum BillingCycle
{
    Annual = 1,
    Monthly = 2
}

public enum SubscriptionStatus
{
    PendingPayment = 1,
    Active = 2,
    Failed = 3,
    Cancelled = 4,
    Expired = 5
}

public enum BenefitRedemptionStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Completed = 4
}

public enum PaymentStatus
{
    Pending = 1,
    Succeeded = 2,
    Failed = 3,
    Refunded = 4
}

public enum PaymentMethodType
{
    CreditCard = 1,
    DigitalWallet = 2,
    BankTransfer = 3
}

public enum UserRole
{
    Admin = 1,
    Patient = 2
}

public enum ProgramIcon
{
    Crown = 1,
    Star = 2,
    Medal = 3,
    Shield = 4,
    Heart = 5,
    Stethoscope = 6,
    Pill = 7,
    MedicalCross = 8,
    Sparkles = 9,
    Award = 10,
    Gift = 11,
    Zap = 12,
    Flame = 13
}

public enum ColorGradient
{
    Purple = 1,
    Gold = 2,
    Silver = 3,
    Bronze = 4,
    Blue = 5,
    Green = 6,
    Red = 7
}
