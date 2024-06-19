using System.Text.RegularExpressions;
using ChessServer.Data.Repositories.Interfaces;
using ChessServer.Domain.Authentication;
using ChessServer.Domain.Dtos;
using ChessServer.Domain.Models;
using ChessServer.WebApi.Authentication.Interfaces;
using ChessServer.WebApi.Controllers.Base;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChessServer.WebApi.Controllers;

[Route("auth")]
public sealed partial class AuthenticationController : BaseController
{
    private readonly IMapper _mapper;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUserRepository _userRepository;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public AuthenticationController(IMapper mapper, IJwtTokenGenerator jwtTokenGenerator,
        IUserRepository userRepository, CancellationTokenSource cancellationTokenSource)
    {
        _mapper = mapper;
        _jwtTokenGenerator = jwtTokenGenerator;
        _userRepository = userRepository;
        _cancellationTokenSource = cancellationTokenSource;
    }

    [HttpPost("register"), AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromQuery] RegisterRequest request)
    {
        if (await _userRepository.GetByEmailAsync(request.Email, _cancellationTokenSource.Token) != null ||
            await _userRepository.GetByUsernameAsync(request.Username, _cancellationTokenSource.Token) != null)
            return Conflict("User already registered.");

        if (ValidateUsername(request.Username) == false || ValidateEmail(request.Email) == false)
            return BadRequest("Invalid username or email.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Username = request.Username,
            Password = request.Password,
            Subscription = new Subscription(SubscriptionType.Basic),
            EloRating = 1500,
        };

        var token = _jwtTokenGenerator.Generate(_mapper.Map<UserJwtDto>(user));

        await _userRepository.AddAsync(user, _cancellationTokenSource.Token);
        await _userRepository.SaveChangesAsync(_cancellationTokenSource.Token);

        return Ok(_mapper.Map<AuthenticationResponse>((user, token)));
    }

    [HttpPost("login"), AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromQuery] LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username, _cancellationTokenSource.Token);

        if (user == null || user.Password != request.Password)
            return BadRequest("Incorrect username or password.");

        var token = _jwtTokenGenerator.Generate(_mapper.Map<UserJwtDto>(user));

        return Ok(_mapper.Map<AuthenticationResponse>((user, token)));
    }

    public bool ValidateEmail(string email)
    {
        return !string.IsNullOrWhiteSpace(email) && EmailRegex().IsMatch(email);
    }

    public bool ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username) || !char.IsLetter(username.First()))
            return false;
        
        return username.Length >= 5;
    }

    [GeneratedRegex("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$")]
    private static partial Regex EmailRegex();
}