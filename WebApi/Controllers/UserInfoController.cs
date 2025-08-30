using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserInfoController : ControllerBase
{
    [HttpGet]
    [Authorize]
    public IActionResult GetUserInfo()
    {
        var userInfo = new
        {
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
            Claims = User.Claims.ToDictionary(c => c.Type, c => c.Value)
        };
        return Ok(userInfo);
    }
}