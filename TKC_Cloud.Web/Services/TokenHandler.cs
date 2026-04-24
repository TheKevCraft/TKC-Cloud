using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.JSInterop;

namespace TKC_Cloud.Web.Services;

public class TokenHandler : DelegatingHandler
{
    private readonly IJSRuntime _js;
    private readonly IHttpClientFactory _httpFactory;

    public TokenHandler(IJSRuntime js, IHttpClientFactory httpFactory)
    {
        _js = js;
        _httpFactory = httpFactory;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        /*var token = await _js.InvokeAsync<string>("localStorage.getItem", "token");

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);*/

        // 1. Access Token aus sessionStorage
        var token = await _js.InvokeAsync<string>("sessionStorage.getItem", "accessToken");
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, cancellationToken);

        // 2. Bei 401 → Refresh Token nutzen
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var refreshToken = await _js.InvokeAsync<string>("localStorage.getItem", "refreshToken");
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                var client = _httpFactory.CreateClient("Auth");
                var refreshResponse = await client.PostAsJsonAsync("api/auth/refresh", new { RefreshToken = refreshToken });
                if (refreshResponse.IsSuccessStatusCode)
                {
                    var data = await refreshResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                    token = data!["accessToken"];

                    // Access Token in sessionStorage speichern
                    await _js.InvokeVoidAsync("sessionStorage.setItem", "accessToken", token);

                    // Originale Anfrage wiederholen
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    response = await base.SendAsync(request, cancellationToken);
                }
            }
        }

        return response;
    }
}