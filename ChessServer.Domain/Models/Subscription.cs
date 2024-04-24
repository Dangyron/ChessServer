namespace ChessServer.Domain.Models;

public sealed class Subscription : Entity
{
    public SubscriptionType SubscriptionType { get; set; } = SubscriptionType.Basic;
    
    public Subscription(SubscriptionType subscriptionType, Guid? id = null)
        : base(id ?? Guid.NewGuid())
    {
        SubscriptionType = subscriptionType;
    }

    public static readonly Subscription Canceled = new (SubscriptionType.Canceled, Guid.Empty);

    private Subscription()
    {
    }
}