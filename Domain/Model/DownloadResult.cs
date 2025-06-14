namespace YoutubeDownloader.Domain.Model;
public class DownloadResult{
    public string? FileName { get; set; }
    public byte[]? FileSize{get; set;}
    public string? FilePath { get; set; }
}