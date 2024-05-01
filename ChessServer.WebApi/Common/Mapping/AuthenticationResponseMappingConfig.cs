using ChessServer.Domain.Authentication;
using ChessServer.Domain.Models;
using Mapster;

namespace ChessServer.WebApi.Common.Mapping;

public class AuthenticationResponseMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<(User, string), AuthenticationResponse>()
            .Map(dest => dest.Id, src => src.Item1.Id)
            .Map(dest => dest.Username, src => src.Item1.Username)
            .Map(dest => dest.Token, src => src.Item2);
    }
}