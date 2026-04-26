using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TKC_Cloud.Data;

namespace TKC_Cloud.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("search")]
    [Authorize]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(new List<UserDto>());

        var users = await _db.Users
            .Where(u => u.Username.Contains(q))
            .OrderBy(u => u.Username)
            .Take(10)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username 
            })
            .ToListAsync();

        return Ok(users);
    }
}