namespace ChessServer.Domain.Models;

public sealed class User : Entity
{
    public int? Age { get; set; }
    public string? Country { get; set; }
    public Gender? Gender { get; set; }
    public Subscription Subscription { get; set; } = null!;
    public int EloRating { get; set; } = 1500;
    public string Email { get; set; } = null!;
    public bool EmailConfirmed { get; set; }
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;

    public User(Guid id, string username, string email, string password, int? age = null, string? country = null, Gender? gender = null, Subscription? subscription = null) : base(id)
    {
        Username = username;
        Email = email;
        Password = password;
        Age = age;
        Country = country;
        Gender = gender;
        Subscription = subscription ?? new Subscription(SubscriptionType.Basic);
    }

    public User() {}
}