FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ChessServer.WebApi/ChessServer.WebApi.csproj", "ChessServer.WebApi/"]
COPY ["ChessServer.Data/ChessServer.Data.csproj", "ChessServer.Data/"]
COPY ["ChessServer.Domain/ChessServer.Domain.csproj", "ChessServer.Domain/"]
RUN dotnet restore "./ChessServer.WebApi/ChessServer.WebApi.csproj"
COPY . .
WORKDIR "/src/ChessServer.WebApi"
RUN dotnet build "./ChessServer.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ChessServer.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChessServer.WebApi.dll"]
