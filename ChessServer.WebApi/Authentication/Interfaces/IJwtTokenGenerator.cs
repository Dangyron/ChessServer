using ChessServer.Domain.Dtos;

namespace ChessServer.WebApi.Authentication.Interfaces;

public interface IJwtTokenGenerator
{
    string Generate(UserJwtDto userDto);
}