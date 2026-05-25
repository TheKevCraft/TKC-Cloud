namespace TKC_Shared.DTOs;

public class FileEntryDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long Size { get; set; }
    public string? ContentType { get; set; }
}