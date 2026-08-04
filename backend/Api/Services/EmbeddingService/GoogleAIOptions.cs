namespace DocuMind.Api.Services.EmbeddingService;
public class GoogleAIOptions
{
    public const string SectionName = "GoogleAI";

    public string ApiKey { get; set; } = string.Empty;

    public string EmbeddingModel { get; set; } = "gemini-embedding-001";
}