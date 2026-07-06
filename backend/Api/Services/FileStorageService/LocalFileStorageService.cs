using DocuMind.Api.Features.Documents.Upload;
namespace DocuMind.Api.Services.FileStorageService;
public class LocalFileStorageService : IFileStorageService
{
    public LocalFileStorageService()
    {
    }


#region IFileStorageService Members
    public async Task<UploadedDocuments[]> StoreFileAsync(List<IFormFile> files)
    {

        UploadedDocuments[] result = new UploadedDocuments[files.Count];
        // Create the storage folder if it doesn't exist
        var storagePath = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "Documents");
        if (!Directory.Exists(storagePath))
        {
            Directory.CreateDirectory(storagePath);
        }

        var documentIds = new List<Guid>();
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

            documentIds.Add(Guid.Parse(uniqueFileName));
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