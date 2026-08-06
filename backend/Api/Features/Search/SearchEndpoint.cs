using System.Text.Json;
using DocuMind.Api.Services.SearchService;
using Microsoft.AspNetCore.Mvc;

namespace DocuMind.Api.Features.Search;

public static class SearchEndpoint
{
    public static void MapSearchEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/search",
             async (
                 HttpRequest request, [FromBody] SearchRequestDTO searchRequest,
                 ISearchService searchService) =>
             {
                 if (searchRequest == null ||
                     string.IsNullOrWhiteSpace(searchRequest.Question))
                 {
                     return Results.BadRequest("Question is required.");
                 }

                 var results =
                     await searchService.SearchAsync(searchRequest.Question);

                 if (results.Count() == 0)
                 {
                     return Results.InternalServerError("No relevant information found.");
                 }

                 return Results.Ok(new SearchResponseDTO
                 {
                     Results = results
                 });
             });
    }
}