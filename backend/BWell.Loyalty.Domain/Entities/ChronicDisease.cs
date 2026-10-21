using BWell.Loyalty.Domain.Common;

namespace BWell.Loyalty.Domain.Entities;

public class ChronicDisease : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<PatientChronicDisease> Patients { get; set; } = new List<PatientChronicDisease>();
}
