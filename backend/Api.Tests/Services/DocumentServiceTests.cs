using System.Text;
using DocuMind.Api.Common.Models;
using DocuMind.Api.Features.Documents.Upload;
using DocuMind.Api.Services.ChunkRepositoryService;
using DocuMind.Api.Services.DocumentService;
using DocuMind.Api.Services.ExtractAndChunkService;
using DocuMind.Api.Services.FileStorageService;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Api.Tests.Services;

public class DocumentServiceTests
{
    [Fact]
    public async Task UploadDocumentsAsync_ReturnsError_WhenNoFilesProvided()
    {
        var fileStorageService = new Mock<IFileStorageService>();
        var pdfTextExtractor = new Mock<IPdfTextExtractor>();
        var chunkRepository = new Mock<IChunkRepository>();

        var service = new DocumentService(fileStorageService.Object, pdfTextExtractor.Object, chunkRepository.Object);

        var request = new UploadDocumentRequestDTO
        {
            Files = null!
        };

        var response = await service.UploadDocumentsAsync(request);

        response.Error.Should().NotBeNull();
        response.Error!.ErrorMessage.Should().Be(DocuMind.Api.Common.Constants.NoFilesProvidedErrorMessage);
        response.Error.ErrorCode.Should().Be(DocuMind.Api.Common.Constants.NoFilesProvidedErrorCode);
        response.UploadedDocuments.Should().BeEmpty();
    }

    [Fact]
    public async Task UploadDocumentsAsync_ReturnsError_WhenFileCountExceedsLimit()
    {
        var fileStorageService = new Mock<IFileStorageService>();
        var pdfTextExtractor = new Mock<IPdfTextExtractor>();
        var chunkRepository = new Mock<IChunkRepository>();

        var service = new DocumentService(fileStorageService.Object, pdfTextExtractor.Object, chunkRepository.Object);

        var request = new UploadDocumentRequestDTO();
        for (var i = 0; i < 6; i++)
        {
            request.Files.Add(CreateFormFile("document.pdf", 1, "application/pdf"));
        }

        var response = await service.UploadDocumentsAsync(request);

        response.Error.Should().NotBeNull();
        response.Error!.ErrorMessage.Should().Be(DocuMind.Api.Common.Constants.MaxFileUploadLimitErrorMessage);
        response.Error.ErrorCode.Should().Be(DocuMind.Api.Common.Constants.MaxFileUploadLimitErrorCode);
        response.UploadedDocuments.Should().BeEmpty();
    }

    [Fact]
    public async Task UploadDocumentsAsync_ReturnsError_WhenFileIsEmpty()
    {
        var fileStorageService = new Mock<IFileStorageService>();
        var pdfTextExtractor = new Mock<IPdfTextExtractor>();
        var chunkRepository = new Mock<IChunkRepository>();

        var service = new DocumentService(fileStorageService.Object, pdfTextExtractor.Object, chunkRepository.Object);

        var request = new UploadDocumentRequestDTO();
        request.Files.Add(CreateFormFile("empty.pdf", 0, "application/pdf"));

        var response = await service.UploadDocumentsAsync(request);

        response.Error.Should().NotBeNull();
        response.Error!.ErrorMessage.Should().Be(DocuMind.Api.Common.Constants.NoFilesProvidedErrorMessage);
        response.Error.ErrorCode.Should().Be(DocuMind.Api.Common.Constants.NoFilesProvidedErrorCode);
        response.UploadedDocuments.Should().BeEmpty();
    }

    [Fact]
    public async Task UploadDocumentsAsync_ReturnsError_WhenFileFormatIsUnsupported()
    {
        var fileStorageService = new Mock<IFileStorageService>();
        var pdfTextExtractor = new Mock<IPdfTextExtractor>();
        var chunkRepository = new Mock<IChunkRepository>();

        var service = new DocumentService(fileStorageService.Object, pdfTextExtractor.Object, chunkRepository.Object);

        var request = new UploadDocumentRequestDTO();
        request.Files.Add(CreateFormFile("document.txt", 1, "text/plain"));

        var response = await service.UploadDocumentsAsync(request);

        response.Error.Should().NotBeNull();
        response.Error!.ErrorMessage.Should().Be(DocuMind.Api.Common.Constants.UnsupportedFileFormatErrorMessage);
        response.Error.ErrorCode.Should().Be(DocuMind.Api.Common.Constants.UnsupportedFileFormatErrorCode);
        response.UploadedDocuments.Should().BeEmpty();
    }

    [Fact]
    public async Task UploadDocumentsAsync_ReturnsError_WhenFileSizeExceedsLimit()
    {
        var fileStorageService = new Mock<IFileStorageService>();
        var pdfTextExtractor = new Mock<IPdfTextExtractor>();
        var chunkRepository = new Mock<IChunkRepository>();

        var service = new DocumentService(fileStorageService.Object, pdfTextExtractor.Object, chunkRepository.Object);

        var request = new UploadDocumentRequestDTO();
        request.Files.Add(CreateFormFile("big.pdf", 10 * 1024 * 1024 + 1, "application/pdf"));

        var response = await service.UploadDocumentsAsync(request);

        response.Error.Should().NotBeNull();
        response.Error!.ErrorMessage.Should().Be(DocuMind.Api.Common.Constants.FileSizeExceededErrorMessage);
        response.Error.ErrorCode.Should().Be(DocuMind.Api.Common.Constants.FileSizeExceededErrorCode);
        response.UploadedDocuments.Should().BeEmpty();
    }

    [Fact]
    public async Task UploadDocumentsAsync_SavesChunksAndReturnsUploadedDocuments_WhenRequestIsValid()
    {
        var fileStorageService = new Mock<IFileStorageService>();
        var pdfTextExtractor = new Mock<IPdfTextExtractor>();
        var chunkRepository = new Mock<IChunkRepository>();

        var storedDocuments = new[]
        {
            new UploadedDocuments
            {
                DocumentId = Guid.NewGuid(),
                FileName = "sample.pdf",
                FilePath = "Storage/Documents/sample.pdf",
                Status = "Uploaded"
            }
        };

        var extractedChunks = new List<Chunk>
        {
            new Chunk
            {
                ChunkId = Guid.NewGuid(),
                DocumentId = storedDocuments[0].DocumentId,
                ChunkIndex = 0,
                Text = "Extracted text",
                PageNumber = 1
            }
        };

        fileStorageService
            .Setup(s => s.StoreFileAsync(It.IsAny<List<IFormFile>>()))
            .ReturnsAsync(storedDocuments);

        pdfTextExtractor
            .Setup(x => x.ExtractTextFromPdfAsync(storedDocuments))
            .ReturnsAsync(extractedChunks);

        chunkRepository
            .Setup(r => r.SaveChunksAsync(extractedChunks))
            .ReturnsAsync(true);

        var service = new DocumentService(fileStorageService.Object, pdfTextExtractor.Object, chunkRepository.Object);
        var request = new UploadDocumentRequestDTO();
        request.Files.Add(CreateFormFile("sample.pdf", 1, "application/pdf"));

        var response = await service.UploadDocumentsAsync(request);

        response.Error.Should().BeNull();
        response.RequestId.Should().NotBeEmpty();
        response.UploadedDocuments.Should().ContainSingle()
            .Which.FileName.Should().Be("sample.pdf");

        fileStorageService.Verify(s => s.StoreFileAsync(It.IsAny<List<IFormFile>>()), Times.Once);
        pdfTextExtractor.Verify(x => x.ExtractTextFromPdfAsync(storedDocuments), Times.Once);
        chunkRepository.Verify(r => r.SaveChunksAsync(extractedChunks), Times.Once);
    }

    private static IFormFile CreateFormFile(string fileName, long length, string contentType)
    {
        var content = new byte[length];
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, length, "files", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
