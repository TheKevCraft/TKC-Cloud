using System.Net;

namespace TKC_Cloud.Web.Services;

public class ProgressableStreamContent : HttpContent
{
    private readonly Stream _stream;
    private readonly int _bufferSize;
    private readonly IProgress<long>? _progress;

    public ProgressableStreamContent(Stream stream, int bufferSize, IProgress<long>? progress)
    {
        _stream = stream;
        _bufferSize = bufferSize;
        _progress = progress;
    }

    protected override async Task SerializeToStreamAsync(Stream targetStream, TransportContext? context)
    {
        var buffer = new Byte[_bufferSize];
        long uploaded = 0;
        int bytesRead;

        while ((bytesRead = await _stream.ReadAsync(buffer)) > 0)
        {
            await targetStream.WriteAsync(buffer.AsMemory(0, bytesRead));

            uploaded += bytesRead;
            _progress?.Report(uploaded);
        }
    }

    protected override bool TryComputeLength(out long length)
    {
        length = _stream.Length;
        return true;
    }
}