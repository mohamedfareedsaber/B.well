using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BWell.Loyalty.Application.DTOs.Auth;
using BWell.Loyalty.Application.Interfaces;
using BWell.Loyalty.Domain.Entities;
using BWell.Loyalty.Domain.Enums;
using BWell.Loyalty.Infrastructure.Data;
using BWell.Loyalty.Infrastructure.Options;
using BWell.Loyalty.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BWell.Loyalty.Infrastructure.Services;

public class AuthService : IAuthService
{
    public const string DefaultPatientPassword = "Patient@123";

    private readonly BWellDbContext _db;
    private readonly JwtOptions _jwt;

    public AuthService(BWellDbContext db, IOptions<JwtOptions> jwt)
    {
        _db = db;
        _jwt = jwt.Value;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users
            .Include(u => u.PatientProfile)
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive, ct);
        if (user is null) return null;
        if (!PasswordHasher.Verify(request.Password, user.PasswordHash)) return null;
        return CreateToken(user);
    }

    public async Task<LoginResponse> EnsureDefaultAdminAsync(CancellationToken ct = default)
    {
        const string email = "admin@bwell.local";
        var admin = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (admin is null)
        {
            admin = new AppUser
            {
                Email = email,
                FullName = "Admin User",
                Role = UserRole.Admin,
                PasswordHash = PasswordHasher.Hash("Admin@123"),
                IsActive = true
            };
            _db.Users.Add(admin);
            await _db.SaveChangesAsync(ct);
        }

        return CreateToken(admin);
    }

    public async Task EnsurePatientUserAsync(Patient patient, string? plainPassword, CancellationToken ct = default)
    {
        if (patient.AppUserId.HasValue) return;

        var email = string.IsNullOrWhiteSpace(patient.Email)
            ? $"{patient.PatientCode.ToLowerInvariant()}@bwell.local"
            : patient.Email.Trim().ToLowerInvariant();

        var existing = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (existing is not null)
        {
            patient.AppUserId = existing.Id;
            if (existing.Role != UserRole.Admin)
                existing.Role = UserRole.Patient;
            return;
        }

        var user = new AppUser
        {
            Email = email,
            FullName = patient.FullName,
            Role = UserRole.Patient,
            PasswordHash = PasswordHasher.Hash(string.IsNullOrWhiteSpace(plainPassword)
                ? DefaultPatientPassword
                : plainPassword),
            IsActive = true
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        patient.AppUserId = user.Id;
        patient.Email ??= email;
    }

    private LoginResponse CreateToken(AppUser user)
    {
        if (string.IsNullOrWhiteSpace(_jwt.Key) || _jwt.Key.Length < 32)
            throw new InvalidOperationException("Jwt:Key must be at least 32 characters.");

        var expires = DateTime.UtcNow.AddHours(_jwt.ExpiryHours);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        if (user.PatientProfile is not null)
            claims.Add(new Claim("patient_id", user.PatientProfile.Id.ToString()));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: creds);

        return new LoginResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            user.FullName,
            user.Email,
            user.Role.ToString(),
            expires,
            user.PatientProfile?.Id);
    }
}
