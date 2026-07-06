using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DocuMind.Api.Features.Documents.Upload;
using DocuMind.Api.Services.DocumentService;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Api.Tests.Features.Documents;

public class UploadDocumentEndpointTests
{
    [Fact]
    public async Task MapUploadDocumentEndpoint_ReturnsBadRequest_WhenServiceReturnsError()
    {
        // Arrange
        var documentService = new Mock<IDocumentService>();
        documentService
            .Setup(service => service.UploadDocumentsAsync(It.IsAny<UploadDocumentRequestDTO>()))
            .ReturnsAsync(new UploadDocumentResponseDTO
            {
                Error = new ErrorResponse
                {
                    ErrorMessage = "No files provided",
                    ErrorCode = 1001
                }
            });

        await using var app = await CreateAppAsync(documentService.Object);
        using var client = app.GetTestClient();

        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3 });
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(fileContent, "files", "sample.pdf");

        // Act
        var response = await client.PostAsync("/api/documents", form);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var payload = await response.Content.ReadFromJsonAsync<UploadDocumentResponseDTO>();
        payload.Should().NotBeNull();
        payload!.Error.Should().NotBeNull();
        payload.Error!.ErrorMessage.Should().Be("No files provided");
        payload.Error.ErrorCode.Should().Be(1001);
    }

    [Fact]
    public async Task MapUploadDocumentEndpoint_ReturnsCreated_WhenServiceSucceeds()
    {
        // Arrange
        var documentService = new Mock<IDocumentService>();
        documentService
            .Setup(service => service.UploadDocumentsAsync(It.IsAny<UploadDocumentRequestDTO>()))
            .ReturnsAsync(new UploadDocumentResponseDTO
            {
                RequestId = Guid.NewGuid(),
                UploadedDocuments = new[]
                {
                    new UploadedDocuments
                    {
                        DocumentId = Guid.NewGuid(),
                        FileName = "sample.pdf",
                        FilePath = "/tmp/sample.pdf",
                        Status = "Uploaded"
                    }
                }
            });

        await using var app = await CreateAppAsync(documentService.Object);
        using var client = app.GetTestClient();

        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3 });
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(fileContent, "files", "sample.pdf");

        // Act
        var response = await client.PostAsync("/api/documents", form);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = await response.Content.ReadFromJsonAsync<UploadDocumentResponseDTO>();
        payload.Should().NotBeNull();
        payload!.Error.Should().BeNull();
        payload.UploadedDocuments.Should().ContainSingle();
        payload.UploadedDocuments[0].FileName.Should().Be("sample.pdf");
    }

    private static async Task<WebApplication> CreateAppAsync(IDocumentService documentService)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton(documentService);

        var app = builder.Build();
        app.MapUploadDocumentEndpoint();
        await app.StartAsync();

        return app;
    }
}
