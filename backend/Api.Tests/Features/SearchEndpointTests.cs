using System.Net;
using System.Net.Http.Json;
using DocuMind.Api.Features.Search;
using DocuMind.Api.Services.SearchService;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Api.Tests.Features.Search;

public class SearchEndpointTests
{
    [Fact]
    public async Task MapSearchEndpoint_ReturnsBadRequest_WhenQuestionIsMissing()
    {
        // Arrange
        var searchService = new Mock<ISearchService>();
        await using var app = await CreateAppAsync(searchService.Object);
        using var client = app.GetTestClient();

        var request = new { Question = string.Empty };

        // Act
        var response = await client.PostAsJsonAsync("/api/search", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task MapSearchEndpoint_ReturnsInternalServerError_WhenNoResultsFound()
    {
        // Arrange
        var searchService = new Mock<ISearchService>();
        searchService
            .Setup(service => service.SearchAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<SearchResultDTO>());

        await using var app = await CreateAppAsync(searchService.Object);
        using var client = app.GetTestClient();

        var request = new SearchRequestDTO { Question = "What is DocuMind?" };

        // Act
        var response = await client.PostAsJsonAsync("/api/search", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task MapSearchEndpoint_ReturnsOk_WhenResultsExist()
    {
        // Arrange
        var expectedResults = new List<SearchResultDTO>
        {
            new() { Score = 0.95, Text = "Document text 1", PageNumber = 1, ChunkIndex = 0, DocumentId = "doc-1" },
            new() { Score = 0.88, Text = "Document text 2", PageNumber = 2, ChunkIndex = 1, DocumentId = "doc-2" }
        };

        var searchService = new Mock<ISearchService>();
        searchService
            .Setup(service => service.SearchAsync("What is DocuMind?"))
            .ReturnsAsync(expectedResults);

        await using var app = await CreateAppAsync(searchService.Object);
        using var client = app.GetTestClient();

        var request = new SearchRequestDTO { Question = "What is DocuMind?" };

        // Act
        var response = await client.PostAsJsonAsync("/api/search", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<SearchResponseDTO>();
        payload.Should().NotBeNull();
        payload!.Results.Should().BeEquivalentTo(expectedResults);
    }

    private static async Task<WebApplication> CreateAppAsync(ISearchService searchService)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton(searchService);

        var app = builder.Build();
        app.MapSearchEndpoint();
        await app.StartAsync();

        return app;
    }
}
