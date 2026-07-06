namespace DocuMind.Api.Common.Models;
public class Document
{
    public Guid DocumentId { get; set; }
    public Guid RequestId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime UploadedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
}