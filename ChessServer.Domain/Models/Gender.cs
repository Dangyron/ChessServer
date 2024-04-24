using Ardalis.SmartEnum;

namespace ChessServer.Domain.Models;

public sealed class Gender : SmartEnum<Gender>
{
    public static readonly Gender Male = new ("Male", 1);
    public static readonly Gender Female = new ("Female", 2);
    public static readonly Gender Other = new ("Apache attack helicopter", 3);
    
    private Gender(string name, int value) : base(name, value)
    {
    }
}