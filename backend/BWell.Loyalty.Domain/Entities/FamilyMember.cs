using BWell.Loyalty.Domain.Common;

namespace BWell.Loyalty.Domain.Entities;

public class FamilyMember : BaseEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public string FullName { get; set; } = string.Empty;
    public int? Age { get; set; }
    public string? Relationship { get; set; }
    public string? NationalIdOrPassport { get; set; }
}
