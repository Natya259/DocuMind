// implement IfileStorageService's StoreFileAsync method. Store it in a local path : api/storage/documents.
//If the path dos not exist, create it. Return the path of the stored file.
using DocuMind.Api.Features.Documents.Upload;
using DocuMind.Api.Services.FileStorageService;
namespace DocuMind.Api.Services.FileStorageService;
public class LocalFileStorageService : IFileStorageService
{
    private readonly IFileStorageService _fileStorageService;
    public LocalFileStorageService()
    {
    }


#region IFileStorageService Members
    public async Task<UploadedDocuments[]> StoreFileAsync(List<IFormFile> files)
    {

        UploadedDocuments[] result = new UploadedDocuments[files.Count];
        // Create the storage folder if it doesn't exist
        var storagePath = Path.Combine(Directory.GetCurrentDirectory(), "storage", "documents");
        if (!Directory.Exists(storagePath))
        {
            Directory.CreateDirectory(storagePath);
        }

        var documentIds = new List<string>();
        var originalFileNames = new List<string>();
        var filepaths = new List<string>();

        foreach (var file in files)
        {
            // Generate a unique filename using the DocumentId
            string uniqueFileName = Guid.NewGuid().ToString();
            var filePath = Path.Combine(storagePath, uniqueFileName);

            // Save the file to the storage path
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            documentIds.Add(uniqueFileName);
            originalFileNames.Add(file.FileName);
            filepaths.Add(filePath);
        }

        for (int i = 0; i < files.Count; i++)
        {
            result[i] = new UploadedDocuments
            {
                DocumentId = documentIds[i],
                FileName = originalFileNames[i],
                FilePath = filepaths[i],
                Status = "Uploaded"
            };
        }

        return result;
    }

    public async Task DeleteFileAsync(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    #endregion


}