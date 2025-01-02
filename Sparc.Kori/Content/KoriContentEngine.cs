using System.Text;

namespace Sparc.Kori;
public record KoriTextContent(string Id, string? Language = null, string? Text = null, string? Html = null, string? ContentType = null, KoriAudio? Audio = null);
public record KoriAudio(string Url, long Duration, string Voice, ICollection<KoriWord> Subtitles);
public record KoriWord(string Text, long Duration, long Offset);

public class KoriContentEngine(IKoriPages pages, KoriJsEngine js)
{
    public Dictionary<string, KoriTextContent> Content { get; set; } = [];

    public async Task InitializeAsync(Uri uri, string pageTitle)
    {
        var page = await pages.Register(uri.GetLeftPart(UriPartial.Path), pageTitle);
        Content = await http.GetContentAsync(request.Domain, request.Path) ?? [];
    }

    public async Task<Dictionary<string, string>> TranslateAsync(KoriContentRequest request, Dictionary<string, string> nodes)
    {
        if (nodes.Count == 0)
            return nodes;
        
        var keysToTranslate = nodes.Where(x => !Content.ContainsKey(x.Key)).Select(x => x.Key).Distinct().ToList();
        
        if (keysToTranslate.Count == 0)
            return nodes;
        
        var messagesDictionary = keysToTranslate.ToDictionary(key => key, key => nodes[key]);
        var page = await GetOrCreatePage(request);
        
        var content = await http.TranslateAsync(page.Id, messagesDictionary, request.Language);

        if (content == null)
            return nodes;

        foreach (var item in content)
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

    public async Task<KoriTextContent> CreateOrUpdateContentAsync(KoriContentRequest request, string id, string tag, string text)
    {
        // Need to add user ABMode

        var page = await http.GetPageByDomainAndPathAsync(request.Domain, request.Path);
        if (page == null)
            throw new InvalidOperationException("Page not found for the given domain and path.");

        KoriTextContent content;

        if (string.IsNullOrEmpty(id))
        {
            content = await http.CreateContent(request.Domain, request.Path, request.Language, tag, text, "Text");
        }
        else
        {
            content = await http.GetContentByIdAsync(id);

            if (content == null)
            {
                content = await http.CreateContent(request.Domain, request.Path, request.Language, tag, text, "Text");
            }
            else
            {
                await http.SetTextAndHtmlContentAsync(content.Id, text);
                content = await http.GetContentByIdAsync(id);
                //TODO check if it's possible to return value from first http call
            }
        }

        return content;
    }

    public async Task PlayAsync(KoriTextContent content)
    {
        if (content?.Audio?.Url == null)
            return;

        await js.InvokeVoidAsync("playAudio", content.Audio.Url);
    }

    public async Task BeginEditAsync()
    {
        await js.InvokeVoidAsync("edit");
    }

    public async Task ApplyMarkdown(string symbol)
    {
        await js.InvokeVoidAsync("applyMarkdown", symbol);
    }

    public async Task BeginSaveAsync()
    {
        await js.InvokeVoidAsync("save");
    }    

    public async Task CancelAsync()
    {
        await js.InvokeVoidAsync("cancelEdit");
    }

    static string LoremIpsum(int wordCount)
    {
        var words = new[]{"lorem", "ipsum", "dolor", "sit", "amet", "consectetuer",
        "adipiscing", "elit", "sed", "diam", "nonummy", "nibh", "euismod",
        "tincidunt", "ut", "laoreet", "dolore", "magna", "aliquam", "erat"};

        var rand = new Random();
        StringBuilder result = new();

        for (int i = 0; i < wordCount; i++)
        {
            var word = words[rand.Next(words.Length)];
            var punctuation = i == wordCount - 1 ? "." : rand.Next(8) == 2 ? "," : "";

            if (i > 0)
                result.Append($" {word}{punctuation}");
            else
                result.Append($"{word[0].ToString().ToUpper()}{word.AsSpan(1)}");
        }

        return result.ToString();
    }
}
