FROM mcr.microsoft.com/dotnet/sdk:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ChessServer.Data/ChessServer.Data.csproj data/
COPY ChessServer.WebApi/ChessServer.WebApi.csproj api/
COPY ChessServer.Domain core/

RUN dotnet restore "api/ChessServer.WebApi.csproj"
COPY . .
WORKDIR "/src/api"
RUN dotnet build "ChessServer.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChessServer.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChessServer.WebApi.dll"]
