namespace DocuMind.Api.Features.Search;

public class SearchResponseDTO
{
    public List<SearchResultDTO> Results { get; set; } = new();
}

public class SearchResultDTO
{
    public double Score { get; set; }

    public string Text { get; set; } = string.Empty;

    public int PageNumber { get; set; }

    public string DocumentId { get; set; } = string.Empty;

    public int ChunkIndex { get; set; }

    public string DocumentName { get; set; } = string.Empty;
}