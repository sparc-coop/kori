using Ardalis.Specification;

namespace Sparc.Kori;

public record MangoQuery(Dictionary<string, Dictionary<string, object?>> Selector, List<string>? Fields, List<Dictionary<string, string>>? Sort, int? Limit, int? Skip);

public partial class PouchDbSpecification<T>
{
    public MangoQuery Query { get; }

    public PouchDbSpecification(ISpecification<T> spec)
    {
        Query = new MangoQuery(
            GenerateSelector(spec.WhereExpressions), 
            null, 
            GenerateSort(spec.OrderExpressions), 
            spec.Take,
            spec.Skip);
    }

    private Dictionary<string, Dictionary<string, object?>> GenerateSelector(IEnumerable<WhereExpressionInfo<T>> criteria)
    {
        if (!criteria.Any())
            return [];

        var selector = new Dictionary<string, Dictionary<string, object?>>();

        foreach (var where in criteria)
        {
            var visitor = new MangoQueryExpressionVisitor();
            visitor.Visit(where.Filter);
            selector.Add(visitor.Field, new Dictionary<string, object?> { { visitor.Op, visitor.Value } });
        }

        return selector;
    }

    private List<Dictionary<string, string>>? GenerateSort(IEnumerable<OrderExpressionInfo<T>>? orderExpressions)
    {
        if (orderExpressions == null || !orderExpressions.Any())
            return null;

        var sort = orderExpressions.Select(order =>
        {
            var field = order.KeySelector.Body.ToString().Split('.').Last();
            var direction = order.OrderType == OrderTypeEnum.OrderBy ? "asc" : "desc";
            return new Dictionary<string, string> { { field, direction } };
        });

        return sort.ToList();
    }
}
