using Microsoft.AspNetCore.Components.Forms;

namespace TKC_Cloud.Web.Services;

public interface IFileUploadClient
{
    Task UploadFileAsync(IBrowserFile file, IProgress<long>? progress = null);
}

public class FileUploadClient : IFileUploadClient
{
    private readonly HttpClient _http;

    public FileUploadClient(HttpClient httpClient)
    {
        _http = httpClient;
    }

    public async Task UploadFileAsync(IBrowserFile file, IProgress<long>? progress = null)
    {
        using var content = new MultipartFormDataContent();

        var stream = file.OpenReadStream(maxAllowedSize:5L * 1024 * 1024 * 1024); // 1GB

        var streamContent = new ProgressableStreamContent(stream, 81920, progress);

        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

        content.Add(streamContent, "file", file.Name);

        var response = await _http.PostAsync("api/files/upload", content);

        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Upload failed: {response.StatusCode}\n{responseText}");
        }
    }
}