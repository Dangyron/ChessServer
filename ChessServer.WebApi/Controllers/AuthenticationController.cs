using System.Text.RegularExpressions;
using ChessServer.Data.Repositories.Interfaces;
using ChessServer.Domain.Authentication;
using ChessServer.Domain.DtoS;
using ChessServer.Domain.Models;
using ChessServer.WebApi.Authentication.Interfaces;
using ChessServer.WebApi.Common;
using ChessServer.WebApi.Common.Interfaces;
using ChessServer.WebApi.Controllers.Base;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace ChessServer.WebApi.Controllers;

[Route("auth")]
public sealed class AuthenticationController : BaseController
{
    private readonly IMapper _mapper;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUserRepository _userRepository;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly IHubContext<NotificationHub, INotificationHub> _hubContext;

    public AuthenticationController(IMapper mapper, IJwtTokenGenerator jwtTokenGenerator,
        IUserRepository userRepository, CancellationTokenSource cancellationTokenSource, IHubContext<NotificationHub, INotificationHub> hubContext)
    {
        _mapper = mapper;
        _jwtTokenGenerator = jwtTokenGenerator;
        _userRepository = userRepository;
        _cancellationTokenSource = cancellationTokenSource;
        _hubContext = hubContext;
    }

    [HttpPost("register"), AllowAnonymous]
    public async Task<IActionResult> Register([FromQuery] RegisterRequest request)
    {
        if (await _userRepository.GetByEmailAsync(request.Email) != null ||
            await _userRepository.GetByUsernameAsync(request.Username) != null)
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

    private bool ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
        
        var pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        return new Regex(pattern).IsMatch(email);
    }

    private bool ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;
        
        return !char.IsLetter(username.First()) && username.Length >= 5;
    }

    [HttpPost("login"), AllowAnonymous]
    public async Task<IActionResult> Login([FromQuery] LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username, _cancellationTokenSource.Token);
        if (user == null)
            return NotFound("User doesn't exist.");

        var token = _jwtTokenGenerator.Generate(_mapper.Map<UserJwtDto>(user));

        return Ok(_mapper.Map<AuthenticationResponse>((user, token)));
    }
    
    [HttpPost("refresh"), AllowAnonymous]
    public async Task<IActionResult> Refresh([FromQuery] LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username, _cancellationTokenSource.Token);
        if (user == null)
            return NotFound("User doesn't exist.");

        var token = _jwtTokenGenerator.Generate(_mapper.Map<UserJwtDto>(user));

        return Ok(_mapper.Map<AuthenticationResponse>((user, token)));
    }
}