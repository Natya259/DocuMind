namespace DocuMind.Api.Features.Documents.Upload;
public class UploadDocumentResponseDTO
{
    public Guid RequestId { get; set; } = Guid.Empty;
    public UploadedDocuments[] UploadedDocuments { get; set; } = { new UploadedDocuments() };

    public ErrorResponse? Error { get; set; }
}

public class UploadedDocuments
{
    public Guid DocumentId { get; set;} = Guid.Empty;
    public string FileName { get; set;} = string.Empty;
    public string FilePath { get; set;} = string.Empty;
    public string Status { get; set;} = string.Empty;
}

public class ErrorResponse
{
    public string ErrorMessage { get; set; } = string.Empty;
    public int ErrorCode { get; set; }

}