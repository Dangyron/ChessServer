using ChessServer.Data.Data;
using ChessServer.Data.Repositories;
using ChessServer.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace ChessServer.Data.Common;

public static class DependencyInjection
{
    public static IServiceCollection AddDb(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddDbContext<ChessDbContext>(options => options.UseNpgsql(configuration.GetConnectionString(ChessDbContext.ConnectionStringSectionName)));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IGameRepository, GameRepository>();
        
        return services;
    }
}