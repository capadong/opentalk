namespace OpenTalk.Models;

public class UploadFileResult
{
    public string Url { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Name { get; set; } = string.Empty;
    public long Id { get; set; }
}
