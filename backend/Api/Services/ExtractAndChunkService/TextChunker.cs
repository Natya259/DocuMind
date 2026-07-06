using DocuMind.Api.Common.Models;

namespace DocuMind.Api.Services.ExtractAndChunkService;    
public class TextChunker : ITextChunker
{
    public List<Chunk> ChunkText(Guid documentId, string extractedText, int maxChunkSize = 700, int overlap = 50)
    {
        List<Chunk> chunks = new List<Chunk>();
        
        
        
        int startIndex = 0;

        while (startIndex < extractedText.Length)
        {
            Chunk chunk = new Chunk
            {
                DocumentId = documentId,
            };
            
            int endIndex = Math.Min(startIndex + maxChunkSize, extractedText.Length);

            if (endIndex < extractedText.Length)
            {
                int boundaryIndex = endIndex;
                while (boundaryIndex > startIndex && extractedText[boundaryIndex] != ' ')
                {
                    boundaryIndex--;
                }

                if (boundaryIndex > startIndex)
                {
                    endIndex = boundaryIndex;
                }
            }

            chunk.ChunkId = Guid.NewGuid();
            chunk.ChunkIndex = chunks.Count;
            chunk.Text = extractedText.Substring(startIndex, endIndex - startIndex);
            chunks.Add(chunk);

            if (endIndex >= extractedText.Length)
            {
                break;
            }

            int newStart = endIndex - overlap;
            if (newStart < startIndex)
            {
                newStart = startIndex;
            }

            while (newStart > startIndex && extractedText[newStart] != ' ')
            {
                newStart--;
            }

            startIndex = newStart + 1;
        }
        
        return chunks;
    }
}