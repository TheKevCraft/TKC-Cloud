namespace TKC_Shared.Models;

public class FilePagedRequest
{
    public int Skip { get; set; }
    public int Take { get; set; } = 50;

    public string? Search { get; set; }

    public string? SortLabel { get; set; }
    public string? SortDirection { get; set; }
}