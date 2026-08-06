using DocuMind.Api.Features.Search;

namespace DocuMind.Api.Services.SearchService;
public interface ISearchService
{
    Task<List<SearchResultDTO>> SearchAsync(string question);
}