using System.Reflection;
using System.Text;
using ChessServer.Data.Common;
using ChessServer.WebApi.Authentication;
using ChessServer.WebApi.Authentication.Common;
using ChessServer.WebApi.Authentication.Interfaces;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ChessServer.WebApi.Common;

public static class DependencyInjection
{
    public static IServiceCollection AddBaseSetUp(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddControllers()
            .AddNewtonsoftJson();

        services.AddSignalR();
        
        services.AddMapping();

        services.AddDb(configuration);
        
        services.AddAuth(configuration);
        
        return services;
    }
    
    private static IServiceCollection AddMapping(this IServiceCollection services)
    {
        var mapperConfig = TypeAdapterConfig.GlobalSettings;
        mapperConfig.Scan(Assembly.GetExecutingAssembly());

        services.AddSingleton(mapperConfig);
        services.AddScoped<IMapper, ServiceMapper>();
        
        return services;
    }
    
    private static IServiceCollection AddAuth(this IServiceCollection services, ConfigurationManager configuration)
    {
        var jwtTokenSettings = new JwtTokenSettings();
        configuration.Bind(JwtTokenSettings.SectionName, jwtTokenSettings);

        services.AddSingleton(Options.Create(jwtTokenSettings));
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<CancellationTokenSource>();

        services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtTokenSettings.Issuer,
                ValidAudience = jwtTokenSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtTokenSettings.SecretKey)),
            });
        return services;
    }
}
