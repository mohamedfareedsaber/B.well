using BWell.Loyalty.Domain.Common;

namespace BWell.Loyalty.Domain.Entities;

public class Governorate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
