using Ardalis.Specification;

namespace Kori.PageQueries;

public class Search : BlossomQuery<Page>
{
    public Search(string searchTerm) => Query.Where(page =>
         ((page.Domain != null && page.Domain.ToLower().Contains(searchTerm) == true) ||
         (page.Path != null && page.Path.ToLower().Contains(searchTerm) == true)));
}
