using ChessServer.Domain.DtoS;

namespace ChessServer.WebApi.Authentication.Interfaces;

public interface IJwtTokenGenerator
{
    string Generate(UserJwtDto userDto);
}