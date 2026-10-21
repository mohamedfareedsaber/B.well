using BWell.Loyalty.Application.Common;
using BWell.Loyalty.Application.DTOs.Patients;
using BWell.Loyalty.Application.Interfaces;
using BWell.Loyalty.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BWell.Loyalty.Api.Controllers;

[ApiController]
[Authorize(Policy = DependencyInjection.AdminPolicy)]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patients;

    public PatientsController(IPatientService patients) => _patients = patients;

    [HttpGet("overview")]
    public async Task<ActionResult<ApiResponse<PatientsOverviewDto>>> Overview(CancellationToken ct)
        => Ok(ApiResponse<PatientsOverviewDto>.Ok(await _patients.GetOverviewAsync(ct)));

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PatientListItemDto>>>> Search([FromQuery] string? q, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PatientListItemDto>>.Ok(await _patients.SearchAsync(q, ct)));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PatientDetailsDto>>> GetById(Guid id, CancellationToken ct)
    {
        var patient = await _patients.GetByIdAsync(id, ct);
        if (patient is null)
            return NotFound(ApiResponse<PatientDetailsDto>.Fail("Patient not found."));
        return Ok(ApiResponse<PatientDetailsDto>.Ok(patient));
    }
}
