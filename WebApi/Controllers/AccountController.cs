using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebApi.Data;

namespace WebApi.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AccountController(UserManager<User> userManager) : ControllerBase
{
    [HttpGet]
    public IActionResult Welcome()
    {
        if (User.Identity == null || !User.Identity.IsAuthenticated) 
        {
            return Ok("You are NOT authenticated");
        }
        return Ok($"Welcome {User.Identity.Name}!");
    }

    [Authorize]
    [HttpGet("Profile")]
    public async Task<IActionResult> Profile()
    {
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser == null) 
        {
            return BadRequest();
        }
        return Ok(new UserProfile
        {
            Id = currentUser.Id.ToString(),
            FirstName = currentUser.FirstName,
            LastName = currentUser.LastName,
            Email = currentUser.Email,
            Phone = currentUser.PhoneNumber
        });
    }
}
public class UserProfile
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }

    // Add other properties as needed
}