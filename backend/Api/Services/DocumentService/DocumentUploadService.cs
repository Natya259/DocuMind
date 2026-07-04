using DocuMind.Api.Common;
using DocuMind.Api.Features.Documents.Upload;
using DocuMind.Api.Services.FileStorageService;

namespace DocuMind.Api.Services.DocumentService;

public class DocumentUploadService : IDocumentService
{
    private readonly IFileStorageService _fileStorageService;

    public DocumentUploadService(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    public async Task<UploadDocumentResponseDTO> UploadDocumentsAsync(UploadDocumentRequestDTO request)
    {
        ErrorResponse error = new ErrorResponse();
        string errorMessage = string.Empty;
        string errorCode = string.Empty;

        // Validate the request - check if files are provided
        if (request.Files == null || !request.Files.Any())
        {
            var errorResponse = new UploadDocumentResponseDTO
            {
                RequestId = string.Empty,
                UploadedDocuments = Array.Empty<UploadedDocuments>(),
                Error = new ErrorResponse
                {
                    ErrorMessage = Constants.NoFilesProvidedErrorMessage,
                    ErrorCode = Constants.NoFilesProvidedErrorCode
                }

            };
            return errorResponse;

        }

        else if (request.Files.Count > 5)
        {

            var errorResponse = new UploadDocumentResponseDTO
            {
                RequestId = string.Empty,
                UploadedDocuments = Array.Empty<UploadedDocuments>(),
                Error = new ErrorResponse
                {
                    ErrorMessage = Constants.MaxFileUploadLimitErrorMessage,
                    ErrorCode = Constants.MaxFileUploadLimitErrorCode 
                }

            };
            return errorResponse;
        }

        // Validate each file - check for empty files and unsupported formats
        foreach (var file in request.Files)
        {
            if (file.Length == 0)
            {
                var errorResponse = new UploadDocumentResponseDTO
                {
                    RequestId = string.Empty,
                    UploadedDocuments = Array.Empty<UploadedDocuments>(),
                    Error = new ErrorResponse
                    {
                        ErrorMessage = Constants.NoFilesProvidedErrorMessage,
                        ErrorCode = Constants.NoFilesProvidedErrorCode
                    }

                };
                return errorResponse;
            }

            //allow only pdfs
            if (file.ContentType != "application/pdf")
            {
                var errorResponse = new UploadDocumentResponseDTO
                {
                    RequestId = string.Empty,
                    UploadedDocuments = Array.Empty<UploadedDocuments>(),
                    Error = new ErrorResponse
                    {
                        ErrorMessage = Constants.UnsupportedFileFormatErrorMessage,
                        ErrorCode = Constants.UnsupportedFileFormatErrorCode
                    }

                };
                return errorResponse;
            }

            if (file.Length > 10 * 1024 * 1024) // 10 MB limit
            {
                var errorResponse = new UploadDocumentResponseDTO
                {
                    RequestId = string.Empty,
                    UploadedDocuments = Array.Empty<UploadedDocuments>(),
                    Error = new ErrorResponse
                    {
                        ErrorMessage = Constants.FileSizeExceededErrorMessage,
                        ErrorCode = Constants.FileSizeExceededErrorCode
                    }

                };
                return errorResponse;
            }

        }
        
        // Store the files using the file storage service
        var uploadedDocuments = await _fileStorageService.StoreFileAsync(request.Files);

        var response = new UploadDocumentResponseDTO
        {
            RequestId = Guid.NewGuid().ToString(),
            UploadedDocuments = uploadedDocuments
        };

        return response;
    }
}
