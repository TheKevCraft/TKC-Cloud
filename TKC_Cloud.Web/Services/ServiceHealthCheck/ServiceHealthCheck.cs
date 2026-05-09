namespace TKC_Cloud.Web.Services.ServicesHealthCheck;

public class ClouldServiceHealthCheck : IServiceHealthCheck
{
    private readonly HttpClient _http;

    public string Name => "Cloud API";

    public ClouldServiceHealthCheck(HttpClient http)
    {
        _http = http;
    }

    public async Task<ServiceStatus> CheckAsync()
    {
        try
        {
            var response = await _http.GetAsync("/health");

            return new ServiceStatus
            {
                Name = Name,
                IsAvailable = response.IsSuccessStatusCode,
                Messsage = response.IsSuccessStatusCode ? "OK" : "Fehler",
                CheckedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            return new ServiceStatus
            {
                Name = Name,
                IsAvailable = false,
                Messsage = ex.Message,
                CheckedAt = DateTime.UtcNow
            };
        }
    }
}