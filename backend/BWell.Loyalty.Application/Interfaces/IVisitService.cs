using BWell.Loyalty.Application.DTOs.Visits;

namespace BWell.Loyalty.Application.Interfaces;

public interface IVisitService
{
    Task<RecordVisitResultDto> RecordVisitAsync(RecordVisitRequest request, CancellationToken ct = default);
}
