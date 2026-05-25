using System.Security.Claims;

namespace TKC_Cloud.Services;

public interface IUserService
{
    public Guid GetUserId(ClaimsPrincipal user);
}