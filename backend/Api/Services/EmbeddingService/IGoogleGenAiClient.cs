using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocuMind.Api.Services.EmbeddingService;

public interface IGoogleGenAiClient
{
    Task<IReadOnlyList<double>> EmbedContentAsync(string model, string contents);
}
