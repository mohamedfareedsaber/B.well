using BWell.Loyalty.Domain.Common;

namespace BWell.Loyalty.Domain.Entities;

public class Patient : BaseEntity
{
    public string PatientCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int? Age { get; set; }
    public string? Nationality { get; set; }
    public string? NationalIdOrPassport { get; set; }
    public string? MedicalNotes { get; set; }
    public string? StripeCustomerId { get; set; }

    public Guid? AppUserId { get; set; }
    public AppUser? AppUser { get; set; }

    public ICollection<PatientChronicDisease> ChronicDiseases { get; set; } = new List<PatientChronicDisease>();
    public ICollection<FamilyMember> FamilyMembers { get; set; } = new List<FamilyMember>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
