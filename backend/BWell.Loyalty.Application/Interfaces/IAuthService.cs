using BWell.Loyalty.Application.DTOs.Auth;
using BWell.Loyalty.Domain.Entities;

namespace BWell.Loyalty.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<LoginResponse> EnsureDefaultAdminAsync(CancellationToken ct = default);
    Task EnsurePatientUserAsync(Patient patient, string? plainPassword, CancellationToken ct = default);
}
