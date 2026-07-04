using DocuMind.Api.Features.Documents.Upload;

namespace DocuMind.Api.Services.FileStorageService;
public interface IFileStorageService
{
    Task<UploadedDocuments[]> StoreFileAsync(List<IFormFile> files);
    Task DeleteFileAsync(string filePath);
}