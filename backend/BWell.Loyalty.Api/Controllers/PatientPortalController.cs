using System.Security.Claims;
using BWell.Loyalty.Application.Common;
using BWell.Loyalty.Application.DTOs.Portal;
using BWell.Loyalty.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BWell.Loyalty.Api.Controllers;

[ApiController]
[Route("api/portal")]
[Authorize]
public class PatientPortalController : ControllerBase
{
    private readonly IPatientPortalService _portal;

    public PatientPortalController(IPatientPortalService portal) => _portal = portal;

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<PatientPortalDto>>> Me(CancellationToken ct)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized(ApiResponse<PatientPortalDto>.Fail("Invalid token."));

        var data = await _portal.GetMyPortalAsync(userId, ct);
        if (data is null)
            return NotFound(ApiResponse<PatientPortalDto>.Fail("No patient profile linked to this account."));

        return Ok(ApiResponse<PatientPortalDto>.Ok(data));
    }

    [HttpGet("benefit-redemptions")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BenefitRedemptionDto>>>> GetBenefitRedemptions(
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(ApiResponse<IReadOnlyList<BenefitRedemptionDto>>.Fail("Invalid token."));

        var redemptions = await _portal.GetMyBenefitRedemptionsAsync(userId, ct);
        return Ok(ApiResponse<IReadOnlyList<BenefitRedemptionDto>>.Ok(redemptions));
    }

    [HttpPost("benefit-redemptions")]
    public async Task<ActionResult<ApiResponse<BenefitRedemptionDto>>> CreateBenefitRedemption(
        [FromBody] CreateBenefitRedemptionRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(ApiResponse<BenefitRedemptionDto>.Fail("Invalid token."));

        var redemption = await _portal.CreateBenefitRedemptionAsync(userId, request, ct);
        return Ok(ApiResponse<BenefitRedemptionDto>.Ok(redemption, "Redemption request submitted."));
    }

    private bool TryGetUserId(out Guid userId)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(userIdValue, out userId);
    }
}
