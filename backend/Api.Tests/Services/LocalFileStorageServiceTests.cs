using DocuMind.Api.Features.Documents.Upload;
using DocuMind.Api.Services.FileStorageService;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace Api.Tests.Services;

public class LocalFileStorageServiceTests
{
    [Fact]
    public async Task StoreFileAsync_CreatesFilesAndReturnsUploadedDocuments()
    {
        var originalDirectory = Environment.CurrentDirectory;
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        try
        {
            Environment.CurrentDirectory = tempDirectory;
            var service = new LocalFileStorageService();

            var files = new List<IFormFile>
            {
                CreateFormFile("sample.pdf", "hello world", "application/pdf"),
                CreateFormFile("second.pdf", "second content", "application/pdf")
            };

            var result = await service.StoreFileAsync(files);

            result.Should().HaveCount(2);
            result.Select(x => x.FileName).Should().Contain(new[] { "sample.pdf", "second.pdf" });
            result.Select(x => x.Status).Should().AllBeEquivalentTo("Uploaded");
            result.Select(x => x.DocumentId).Should().OnlyHaveUniqueItems();

            foreach (var uploadedDocument in result)
            {
                File.Exists(uploadedDocument.FilePath).Should().BeTrue();
                var savedText = await File.ReadAllTextAsync(uploadedDocument.FilePath);
                savedText.Should().Contain(uploadedDocument.FileName.StartsWith("sample") ? "hello world" : "second content");
            }

            var storageDirectory = Path.Combine(tempDirectory, "Storage", "Documents");
            Directory.Exists(storageDirectory).Should().BeTrue();
        }
        finally
        {
            Environment.CurrentDirectory = originalDirectory;
            Directory.Delete(tempDirectory, true);
        }
    }

    [Fact]
    public async Task DeleteFileAsync_RemovesExistingFile()
    {
        var originalDirectory = Environment.CurrentDirectory;
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        try
        {
            Environment.CurrentDirectory = tempDirectory;
            var storageDirectory = Path.Combine(tempDirectory, "Storage", "Documents");
            Directory.CreateDirectory(storageDirectory);
            var filePath = Path.Combine(storageDirectory, "delete-me.txt");
            await File.WriteAllTextAsync(filePath, "delete this");

            var service = new LocalFileStorageService();
            await service.DeleteFileAsync(filePath);

            File.Exists(filePath).Should().BeFalse();
        }
        finally
        {
            Environment.CurrentDirectory = originalDirectory;
            Directory.Delete(tempDirectory, true);
        }
    }

    [Fact]
    public async Task DeleteFileAsync_DoesNotThrow_WhenFileDoesNotExist()
    {
        var originalDirectory = Environment.CurrentDirectory;
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        try
        {
            Environment.CurrentDirectory = tempDirectory;
            var missingFilePath = Path.Combine(tempDirectory, "Storage", "Documents", "missing.pdf");
            var service = new LocalFileStorageService();

            await service.DeleteFileAsync(missingFilePath);
        }
        finally
        {
            Environment.CurrentDirectory = originalDirectory;
            Directory.Delete(tempDirectory, true);
        }
    }

    private static IFormFile CreateFormFile(string fileName, string content, string contentType)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "files", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
