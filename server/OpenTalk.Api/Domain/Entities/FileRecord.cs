namespace OpenTalk.Domain.Entities;

public class FileRecord
{
    public long Id { get; set; }
    public string FileName { get; set; } = "";
    public string FilePath { get; set; } = "";
    public string FileType { get; set; } = "";
    public long Size { get; set; }
    public long? UploaderId { get; set; }
    public string? UploaderName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
