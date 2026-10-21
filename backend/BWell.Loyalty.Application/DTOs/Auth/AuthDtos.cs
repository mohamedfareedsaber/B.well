namespace BWell.Loyalty.Application.DTOs.Auth;

public record LoginRequest(string Email, string Password);
public record LoginResponse(
    string Token,
    string FullName,
    string Email,
    string Role,
    DateTime ExpiresAtUtc,
    Guid? PatientId = null);
