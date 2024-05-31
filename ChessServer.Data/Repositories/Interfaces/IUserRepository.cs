using ChessServer.Domain.Models;

namespace ChessServer.Data.Repositories.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task UpdateAsync(User entity, CancellationToken? cancellationToken = default);
}