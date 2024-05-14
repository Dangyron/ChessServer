using ChessServer.Data.Repositories.Interfaces;
using ChessServer.Domain.Authentication;
using ChessServer.Domain.DtoS;
using ChessServer.Domain.Models;
using ChessServer.WebApi.Authentication;
using ChessServer.WebApi.Authentication.Interfaces;
using ChessServer.WebApi.Common.Mapping;
using ChessServer.WebApi.Controllers;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace ChessServer.WebApi.Tests.Controllers;

public sealed class AuthenticationControllerTests
{
    private readonly IMapper _mapper;
    
    public AuthenticationControllerTests()
    {
        var services = new ServiceCollection();

        var mapperConfig = TypeAdapterConfig.GlobalSettings;
        mapperConfig.Apply(new AuthenticationResponseMappingConfig());

        services.AddSingleton(mapperConfig);
     
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IMapper, ServiceMapper>();
        var serviceProvider = services.BuildServiceProvider();
        _mapper = serviceProvider.GetRequiredService<IMapper>();
    }
    
    [Fact]
    public async Task Register_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        var jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
        var userRepository = Substitute.For<IUserRepository>();
        var cancellationTokenSource = new CancellationTokenSource();
        
        var controller = new AuthenticationController(
            _mapper, jwtTokenGenerator, userRepository, cancellationTokenSource);
        
        var registerRequest = new RegisterRequest
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "password123"
        };

        userRepository.GetByEmailAsync(Arg.Any<string>(), cancellationTokenSource.Token).Returns(Task.FromResult<User?>(null));
        userRepository.GetByUsernameAsync(Arg.Any<string>(), cancellationTokenSource.Token).Returns(Task.FromResult<User?>(null));
        
        jwtTokenGenerator.Generate(Arg.Any<UserJwtDto>()).Returns("token");
        // Act
        var result = await controller.Register(registerRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var authenticationResponse = Assert.IsType<AuthenticationResponse>(okResult.Value);
        Assert.Equal("testuser", authenticationResponse.Username);
        Assert.Equal("token", authenticationResponse.Token);
        await userRepository.Received(1).AddAsync(Arg.Any<User>(), cancellationTokenSource.Token);
        await userRepository.Received(1).SaveChangesAsync(cancellationTokenSource.Token);
    }
    
    [Fact]
    public async Task Login_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        var jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
        var userRepository = Substitute.For<IUserRepository>();
        var cancellationTokenSource = new CancellationTokenSource();
        
        var controller = new AuthenticationController(
            _mapper, jwtTokenGenerator, userRepository, cancellationTokenSource);
        
        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "password123"
        };

        userRepository.GetByUsernameAsync(Arg.Any<string>(), cancellationTokenSource.Token).Returns(Task.FromResult<User?>(new User
        {
            Username = "testuser",
            Password = "password123"
        }));
        
        jwtTokenGenerator.Generate(Arg.Any<UserJwtDto>()).Returns("token");
        // Act
        var result = await controller.Login(loginRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var authenticationResponse = Assert.IsType<AuthenticationResponse>(okResult.Value);
        Assert.Equal("testuser", authenticationResponse.Username);
        Assert.Equal("token", authenticationResponse.Token);
    }

    [Fact]
    public void ValidateEmail_ValidRequest_ReturnsTrue()
    {
        // Arrange
        var jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
        var userRepository = Substitute.For<IUserRepository>();
        var cancellationTokenSource = new CancellationTokenSource();

        var controller = new AuthenticationController(
            _mapper, jwtTokenGenerator, userRepository, cancellationTokenSource);

        var email = "test@example.com";
        // Act
        var result = controller.ValidateEmail(email);

        // Assert
        Assert.True(Assert.IsType<bool>(result));
    }
    
    [Fact]
    public void ValidateEmail_InvalidRequest_ReturnsFalse()
    {
        // Arrange
        var jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
        var userRepository = Substitute.For<IUserRepository>();
        var cancellationTokenSource = new CancellationTokenSource();

        var controller = new AuthenticationController(
            _mapper, jwtTokenGenerator, userRepository, cancellationTokenSource);

        var email = "test@examplecom";
        // Act
        var result = controller.ValidateEmail(email);

        // Assert
        Assert.False(Assert.IsType<bool>(result));
    }
    
    [Fact]
    public void ValidateUsername_ValidRequest_ReturnsTrue()
    {
        // Arrange
        var jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
        var userRepository = Substitute.For<IUserRepository>();
        var cancellationTokenSource = new CancellationTokenSource();

        var controller = new AuthenticationController(
            _mapper, jwtTokenGenerator, userRepository, cancellationTokenSource);

        var username = "asdfg";
        // Act
        var result = controller.ValidateUsername(username);

        // Assert
        Assert.True(Assert.IsType<bool>(result));
    }
    
    [Fact]
    public void ValidateUsername_InvalidRequest_ReturnsFalse()
    {
        // Arrange
        var jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
        var userRepository = Substitute.For<IUserRepository>();
        var cancellationTokenSource = new CancellationTokenSource();

        var controller = new AuthenticationController(
            _mapper, jwtTokenGenerator, userRepository, cancellationTokenSource);

        var username = "1asdfg";
        // Act
        var result = controller.ValidateUsername(username);

        // Assert
        Assert.False(Assert.IsType<bool>(result));
    }
}