using Ardalis.Specification;

namespace Kori.PageQueries;

public class Search : BlossomQuery<Page>
{
    public Search(string searchTerm) => Query.Where(page =>
         ((page.Domain != null && page.Domain.ToLower().Contains(searchTerm) == true) ||
         (page.Path != null && page.Path.ToLower().Contains(searchTerm) == true)));
}

public class GetByDomainAndPath : BlossomQuery<Page>
{
    public GetByDomainAndPath(string domain, string? path = null) => Query.Where(page =>
        (page.Domain != null && page.Domain.ToLower() == domain.ToLower()) &&
        (path == null || (page.Path != null && page.Path.ToLower() == path!.ToLower())));
}