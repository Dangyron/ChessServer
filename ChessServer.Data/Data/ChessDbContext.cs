using ChessServer.Data.Common.ModelTypeConfigurations;
using ChessServer.Domain.Models;
using Microsoft.EntityFrameworkCore;
using SmartEnum.EFCore;

namespace ChessServer.Data.Data;

public sealed class ChessDbContext : DbContext
{
    public const string ConnectionStringSectionName = "psqlConnection";
    
    public ChessDbContext(DbContextOptions<ChessDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Game> Games { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserTypeConfiguration());
        modelBuilder.ApplyConfiguration(new GameTypeConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}   