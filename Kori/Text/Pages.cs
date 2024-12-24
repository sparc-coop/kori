using Sparc.Blossom.Api;

namespace Kori;

public class Pages(BlossomAggregateOptions<Page> options) : BlossomAggregate<Page>(options)
{
    public BlossomQuery<Page> Search(string searchTerm) => Query().Where(page =>
         ((page.Domain != null && page.Domain.ToLower().Contains(searchTerm) == true) ||
         (page.Path != null && page.Path.ToLower().Contains(searchTerm) == true)));

   public BlossomQuery<Page> GetByDomainAndPath(string domain, string? path = null) => Query().Where(page =>
        (page.Domain != null && page.Domain.ToLower() == domain.ToLower()) &&
        (path == null || (page.Path != null && page.Path.ToLower() == path!.ToLower())));
}