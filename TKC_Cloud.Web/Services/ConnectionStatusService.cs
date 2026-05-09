using System.Net.Http.Json;
using TKC_Shared.DTOs;

namespace TKC_Cloud.Web.Services;

public class ConnectionStatusService
{
    public readonly HttpClient _httpClient;

    public ApiHealthInfo? CloudInfo { get; private set; }
    public bool IsCloudOnline => CloudInfo?.Status == "Healhty";

    public event Action? OnStatusChanged;

    public ConnectionStatusService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task CheckAsync()
    {
        bool oldStatus = IsCloudOnline;

        try
        {
            CloudInfo = await _httpClient.GetFromJsonAsync<ApiHealthInfo>("api/health");
        }
        catch
        {
            
        }

        if (oldStatus != IsCloudOnline)
        {
            OnStatusChanged?.Invoke();
        }
    }
}
