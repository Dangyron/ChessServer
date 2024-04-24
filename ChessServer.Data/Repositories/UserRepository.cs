using System.Diagnostics.CodeAnalysis;
using ChessServer.Data.Data;
using ChessServer.Data.Repositories.Interfaces;
using ChessServer.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ChessServer.Data.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ChessDbContext dbContext) : base(dbContext)
    {
    }

    public Task<User?> GetByEmailAsync([NotNull]string email, CancellationToken cancellationToken = default)
    {
        return Set.FirstOrDefaultAsync(p => p.Email == email, cancellationToken);
    }

    public Task<User?> GetByUsernameAsync([NotNull]string username, CancellationToken cancellationToken = default)
    {
        return Set.FirstOrDefaultAsync(p => p.Email == username, cancellationToken);
    }

    public async Task UpdateAsync(User entity, CancellationToken? cancellationToken = default)
    {
        var user = await Set.FirstOrDefaultAsync(p => p.Id == entity.Id, cancellationToken ?? default);
        
        if (user == null)
        {
            throw new ArgumentException("User doesn't exist.");
        }

        user.Age = entity.Age;
        user.Username = entity.Username;
        user.Country = entity.Country;
        user.Email = entity.Email;
        user.Gender = entity.Gender;
        user.Password = entity.Password;
        user.EmailConfirmed = entity.EmailConfirmed;
        user.Subscription = entity.Subscription;
    }

}