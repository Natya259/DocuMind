using System.Text.Json;
using DocuMind.Api.Common.Models;

namespace DocuMind.Api.Services.ChunkRepositoryService;    

// write logic to store the content of the list of chunks into /api/Storage/Chunks in a json format
// so every list of chunks get stored in a single json

public class ChunkRepository : IChunkRepository
{
    private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "Chunks");
    
    public ChunkRepository()
    {
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    public async Task<bool> SaveChunksAsync(List<Chunk> chunks)
    {
        if (!chunks.Any())
        {
            throw new ArgumentException("The list of chunks is empty.");
        }
        var fileName = $"chunks_{chunks[0].DocumentId}.json";
        var filePath = Path.Combine(_storagePath, fileName);

        var json = System.Text.Json.JsonSerializer.Serialize(chunks);
        await File.WriteAllTextAsync(filePath, json);
        return true;
    }

    //write another method to load chunks from the path
    public async Task<List<Chunk>> LoadAllChunksAsync()
    {
        var chunkDirectory = Path.Combine(
        Directory.GetCurrentDirectory(),
        "Storage",
        "Chunks");

    var chunks = new List<Chunk>();

    foreach (var file in Directory.GetFiles(chunkDirectory, "*.json"))
    {
        var json = await File.ReadAllTextAsync(file);

        var documentChunks =
            JsonSerializer.Deserialize<List<Chunk>>(json);

        if (documentChunks != null)
        {
            chunks.AddRange(documentChunks);
        }
    }

    return chunks;
    }
}