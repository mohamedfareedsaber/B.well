using BWell.Loyalty.Domain.Common;

namespace BWell.Loyalty.Domain.Entities;

public class DeliveryAddress : BaseEntity
{
    public Guid SubscriptionId { get; set; }
    public Subscription Subscription { get; set; } = null!;

    public string Country { get; set; } = "Egypt";
    public string Governorate { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string AddressDetails { get; set; } = string.Empty;
}
