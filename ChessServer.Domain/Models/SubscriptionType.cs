using Ardalis.SmartEnum;

namespace ChessServer.Domain.Models;

public sealed class SubscriptionType : SmartEnum<SubscriptionType>
{
    public static readonly SubscriptionType Basic = new(nameof(Basic), 0);
    public static readonly SubscriptionType Premium = new(nameof(Premium), 1);
    public static readonly SubscriptionType Canceled = new(nameof(Canceled), -1);

    private SubscriptionType(string name, int value) : base(name, value)
    {
    }
}