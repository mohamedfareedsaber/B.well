namespace BWell.Loyalty.Application.DTOs.Visits;

public record RecordVisitRequest(
    Guid PatientId,
    Guid MedicalServiceId,
    Guid? SubscriptionId,
    string? Notes);

public record RecordVisitResultDto(
    Guid VisitId,
    Guid PatientId,
    string MedicalServiceName,
    int VisitSequenceForService,
    decimal DiscountAppliedPercent,
    string Message);
