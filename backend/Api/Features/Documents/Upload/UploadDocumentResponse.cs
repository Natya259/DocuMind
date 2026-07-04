namespace DocuMind.Api.Features.Documents.Upload;
public class UploadDocumentResponseDTO
{
    public string RequestId { get; set; } = string.Empty;
    public UploadedDocuments[] UploadedDocuments { get; set; } = { new UploadedDocuments() };

    public ErrorResponse? Error { get; set; }
}

public class UploadedDocuments
{
    public string DocumentId { get; set;} = string.Empty;
    public string FileName { get; set;} = string.Empty;
    public string FilePath { get; set;} = string.Empty;
    public string Status { get; set;} = string.Empty;
}

public class ErrorResponse
{
    public string ErrorMessage { get; set; } = string.Empty;
    public int ErrorCode { get; set; }

}