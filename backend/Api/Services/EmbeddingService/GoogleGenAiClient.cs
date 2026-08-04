using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Options;

namespace DocuMind.Api.Services.EmbeddingService;

public class GoogleGenAiClient : IGoogleGenAiClient, IDisposable, IAsyncDisposable
{
    private readonly Client _client;
    private bool _disposed;

    public GoogleGenAiClient(IOptions<GoogleAIOptions> options)
    {
        var config = options.Value;
        if (string.IsNullOrWhiteSpace(config.ApiKey))
        {
            throw new ArgumentException("GoogleAI:ApiKey must be configured.", nameof(config.ApiKey));
        }

        _client = new Client(apiKey: config.ApiKey);
    }

    public async Task<IReadOnlyList<double>> EmbedContentAsync(string model, string contents)
    {
        var response = await _client.Models.EmbedContentAsync(model, contents, new EmbedContentConfig(), CancellationToken.None);
        return response?.Embeddings?.Select(e => e.Values).FirstOrDefault() ?? new List<double>();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _client.Dispose();
        _disposed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        await _client.DisposeAsync();
        _disposed = true;
    }
}