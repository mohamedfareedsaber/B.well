using BWell.Loyalty.Application.Common;
using BWell.Loyalty.Application.DTOs.Visits;
using BWell.Loyalty.Application.Interfaces;
using BWell.Loyalty.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BWell.Loyalty.Api.Controllers;

[ApiController]
[Authorize(Policy = DependencyInjection.AdminPolicy)]
[Route("api/[controller]")]
public class VisitsController : ControllerBase
{
    private readonly IVisitService _visits;

    public VisitsController(IVisitService visits) => _visits = visits;

    [HttpPost]
    public async Task<ActionResult<ApiResponse<RecordVisitResultDto>>> Record([FromBody] RecordVisitRequest request, CancellationToken ct)
    {
        var result = await _visits.RecordVisitAsync(request, ct);
        return Ok(ApiResponse<RecordVisitResultDto>.Ok(result, result.Message));
    }
}
