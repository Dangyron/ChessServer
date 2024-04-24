using ChessServer.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChessServer.Data.Common.ModelTypeConfigurations;

public class GameTypeConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.ToTable("Games");

        builder.HasKey(game => game.Id);
        builder.Property(u => u.Id)
            .ValueGeneratedNever();

        builder.Property(game => game.BlackPlayerId).IsRequired();
        builder.Property(game => game.WhitePlayerId).IsRequired();

        builder.Property(game => game.Fen).IsRequired();
        builder.Property(game => game.StartTime).IsRequired();
        builder.Property(game => game.EndTime).IsRequired();
        /*builder.Property(game => game.Moves);*/
        builder.Property(game => game.Result).IsRequired();
    }
}