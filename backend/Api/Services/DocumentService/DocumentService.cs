using DocuMind.Api.Common;
using DocuMind.Api.Common.Models;
using DocuMind.Api.Features.Documents.Upload;
using DocuMind.Api.Services.ChunkRepositoryService;
using DocuMind.Api.Services.ExtractAndChunkService;
using DocuMind.Api.Services.FileStorageService;

namespace DocuMind.Api.Services.DocumentService;

public class DocumentService : IDocumentService
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IPdfTextExtractor _pdfTextExtractor;
    private readonly IChunkRepository _chunkRepository;

    public DocumentService(IFileStorageService fileStorageService, IPdfTextExtractor pdfTextExtractor, IChunkRepository chunkRepository)
    {
        _fileStorageService = fileStorageService;
        _pdfTextExtractor = pdfTextExtractor;
        _chunkRepository = chunkRepository;
    }

    public async Task<UploadDocumentResponseDTO> UploadDocumentsAsync(UploadDocumentRequestDTO request)
    {
        // Validate the request - check if files are provided
        if (request.Files == null || !request.Files.Any())
        {
            var errorResponse = new UploadDocumentResponseDTO
            {
                RequestId = Guid.Empty,
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
                RequestId = Guid.Empty,
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
                    RequestId = Guid.Empty,
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
                    RequestId = Guid.Empty,
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
                    RequestId = Guid.Empty,
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

        //call the pdfTextExtractor class to extract text from the uploaded pdfs and store it in the database
        var chunks = await _pdfTextExtractor.ExtractTextFromPdfAsync(uploadedDocuments);

        //save the chunk using ChunkRepository's SaveChunks method
        await _chunkRepository.SaveChunksAsync(chunks);

        var response = new UploadDocumentResponseDTO
        {
            RequestId = Guid.NewGuid(),
            UploadedDocuments = uploadedDocuments
        };      

        return response;
    }
}
