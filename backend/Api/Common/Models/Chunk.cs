namespace DocuMind.Api.Common.Models;
public class Chunk
{
    public Guid ChunkId { get; set; }

    public Guid DocumentId { get; set; }

    public int ChunkIndex { get; set; }

    public string Text { get; set; } = string.Empty;

    public List<float>? Embedding { get; set; }

    public int? PageNumber { get; set; }

    public string? DocumentName { get; set; }
}