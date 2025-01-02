namespace Kori;

public class Contents(BlossomAggregateOptions<Content> options, KoriTranslator translator) : BlossomAggregate<Content>(options)
{
    public BlossomQuery<Content> Search(string searchTerm) => Query().Where(content =>
         ((content.Text != null && content.Text.ToLower().Contains(searchTerm) == true) ||
         (content.Tag != null && content.Tag.ToLower().Contains(searchTerm) == true) ||
         (content.Domain != null && content.Domain.ToLower().Contains(searchTerm) == true) ||
         (content.Path != null && content.Path.ToLower().Contains(searchTerm) == true)));

    public BlossomQuery<Content> GetAll(string pageId) => Query().Where(content => content.PageId == pageId);

    public async Task<IEnumerable<Content>> Translate(string pageId, Dictionary<string, string> nodes, string language)
    {
        if (nodes.Count == 0)
            return [];

        var keysToTranslate = nodes
            .Where(x => !Content.ContainsKey(x.Key))
            .Select(x => x.Key)
            .Distinct()
            .ToList();
        if (keysToTranslate.Count == 0)
            return nodes;
        var messagesDictionary = keysToTranslate.ToDictionary(key => key, key => nodes[key]);
        var translatedContent = await translator.TranslateAsync(messagesDictionary, language);
        if (translatedContent == null)
            return nodes;
        foreach (var item in translatedContent)
        {
            Content[item.Value.Id] = item.Value;
        }
        foreach (var key in nodes.Keys.ToList())
        {
            if (Content.TryGetValue(key, out KoriTextContent? value))
            {
                nodes[key] = value.Text;
            }
        }
        return nodes;
    }
}
