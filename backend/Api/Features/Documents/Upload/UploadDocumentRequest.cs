namespace DocuMind.Api.Features.Documents.Upload;
public class UploadDocumentRequestDTO
{
    public List<IFormFile> Files { get; set; } = new();
}