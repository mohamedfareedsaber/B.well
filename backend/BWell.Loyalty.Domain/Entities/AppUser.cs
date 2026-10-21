using BWell.Loyalty.Domain.Common;
using BWell.Loyalty.Domain.Enums;

namespace BWell.Loyalty.Domain.Entities;

public class AppUser : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Patient;
    public bool IsActive { get; set; } = true;

    public Patient? PatientProfile { get; set; }
}
