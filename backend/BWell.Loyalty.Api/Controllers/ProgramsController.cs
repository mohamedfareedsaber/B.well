using BWell.Loyalty.Application.Common;
using BWell.Loyalty.Application.DTOs.Programs;
using BWell.Loyalty.Application.Interfaces;
using BWell.Loyalty.Domain.Enums;
using BWell.Loyalty.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BWell.Loyalty.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProgramsController : ControllerBase
{
    private readonly ILoyaltyProgramService _programs;

    public ProgramsController(ILoyaltyProgramService programs) => _programs = programs;

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LoyaltyProgramDto>>>> GetAll(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LoyaltyProgramDto>>.Ok(await _programs.GetAllAsync(ct)));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoyaltyProgramDto>>> GetById(Guid id, CancellationToken ct)
    {
        var program = await _programs.GetByIdAsync(id, ct);
        if (program is null)
            return NotFound(ApiResponse<LoyaltyProgramDto>.Fail("Program not found."));
        return Ok(ApiResponse<LoyaltyProgramDto>.Ok(program));
    }

    [HttpGet("pricing")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PricingCardDto>>>> GetPricing(
        [FromQuery] PlanType planType = PlanType.Individual,
        CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<PricingCardDto>>.Ok(await _programs.GetPricingCardsAsync(planType, ct)));

    [Authorize(Policy = DependencyInjection.AdminPolicy)]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<LoyaltyProgramDto>>> Create([FromBody] CreateLoyaltyProgramRequest request, CancellationToken ct)
    {
        var created = await _programs.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<LoyaltyProgramDto>.Ok(created));
    }

    [Authorize(Policy = DependencyInjection.AdminPolicy)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<LoyaltyProgramDto>>> Update(Guid id, [FromBody] UpdateLoyaltyProgramRequest request, CancellationToken ct)
    {
        var updated = await _programs.UpdateAsync(id, request, ct);
        if (updated is null)
            return NotFound(ApiResponse<LoyaltyProgramDto>.Fail("Program not found."));
        return Ok(ApiResponse<LoyaltyProgramDto>.Ok(updated));
    }

    [Authorize(Policy = DependencyInjection.AdminPolicy)]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _programs.DeleteAsync(id, ct);
        if (!deleted)
            return NotFound(ApiResponse<object>.Fail("Program not found."));
        return Ok(ApiResponse<object>.Ok(new { deleted = true }));
    }
}
