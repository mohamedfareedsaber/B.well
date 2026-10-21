namespace BWell.Loyalty.Domain.Entities;

public class PatientChronicDisease
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public Guid ChronicDiseaseId { get; set; }
    public ChronicDisease ChronicDisease { get; set; } = null!;
}
