using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TKC_Cloud.Services;

public class UserService : IUserService
{
    public Guid GetUserId(ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !Guid.TryParse(claim.Value, out var userId))
            throw new UnauthorizedAccessException();
    
        return Guid.Parse(claim.Value);
    }
}