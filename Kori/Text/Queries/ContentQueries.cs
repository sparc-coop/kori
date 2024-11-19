using Ardalis.Specification;
using Kori;
using Sparc.Blossom.Data;

namespace Kori.ContentQueries;

public class Search : BlossomQuery<Content>
{
    public Search(string searchTerm) => Query.Where(content =>           
        ((content.Text != null && content.Text.ToLower().Contains(searchTerm) == true) ||
         (content.Tag != null && content.Tag.ToLower().Contains(searchTerm) == true) ||
         (content.Domain != null && content.Domain.ToLower().Contains(searchTerm) == true) ||
         (content.Path != null && content.Path.ToLower().Contains(searchTerm) == true)));
}