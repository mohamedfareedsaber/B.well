using BWell.Loyalty.Domain.Common;

namespace BWell.Loyalty.Domain.Entities;

public class MedicalService : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ProgramServiceDiscount> ProgramDiscounts { get; set; } = new List<ProgramServiceDiscount>();
}
