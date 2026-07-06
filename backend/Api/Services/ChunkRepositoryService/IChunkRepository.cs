using DocuMind.Api.Common.Models;

namespace DocuMind.Api.Services.ChunkRepositoryService;
public interface IChunkRepository
{
    Task<bool> SaveChunksAsync(List<Chunk> chunks);
    Task<List<Chunk>> LoadChunksAsync(string fileName);
}