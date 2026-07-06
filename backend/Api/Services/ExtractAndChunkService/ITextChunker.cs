using DocuMind.Api.Common.Models;

namespace DocuMind.Api.Services.ExtractAndChunkService;
public interface ITextChunker
{
    List<Chunk> ChunkText(Guid documentId, string extractedText, int maxChunkSize = 700, int overlap = 50);
}