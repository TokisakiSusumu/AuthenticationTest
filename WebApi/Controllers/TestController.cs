using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("public")]
        public IActionResult GetPublicData()
        {
            return Ok(new
            {
                message = "This is public data - no auth required",
                timestamp = DateTime.UtcNow
            });
        }

        [Authorize]
        [HttpGet("protected")]
        public IActionResult GetProtectedData()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            return Ok(new
            {
                message = "This is protected data - auth required",
                userId = userId,
                email = email,
                timestamp = DateTime.UtcNow,
                claims = User.Claims.Select(c => new { c.Type, c.Value })
            });
        }

        [Authorize]
        [HttpGet("whoami")]
        public IActionResult WhoAmI()
        {
            return Ok(new
            {
                isAuthenticated = User.Identity?.IsAuthenticated ?? false,
                name = User.Identity?.Name,
                authenticationType = User.Identity?.AuthenticationType,
                claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            });
        }
    }
}