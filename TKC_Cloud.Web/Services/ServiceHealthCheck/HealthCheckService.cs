namespace TKC_Cloud.Web.Services.ServicesHealthCheck;

public class HealthCheckService
{
    private readonly IEnumerable<IServiceHealthCheck> _checks;

    public HealthCheckService(IEnumerable<IServiceHealthCheck> checks)
    {
        _checks = checks;
    }

    public async Task<List<ServiceStatus>> CheckAllAsync()
    {
        var results = new List<ServiceStatus>();

        foreach ( var check in _checks)
        {
            results.Add(await check.CheckAsync());
        }

        return results;
    }
}