using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace TKC_Cloud.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public HealthController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var version = Assembly.GetExecutingAssembly()
            .GetName()
            .Version?
            .ToString() ?? "Unknown";

        return Ok(new
        {
            status = "Healthy",
            name = "Cloud API",
            version,
            environment = _env.EnvironmentName,
            timestamp = DateTime.UtcNow
        });
    }
}