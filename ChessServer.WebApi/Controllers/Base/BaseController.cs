using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChessServer.WebApi.Controllers.Base;

[ApiController]
[Authorize]
public class BaseController : ControllerBase
{
    
}