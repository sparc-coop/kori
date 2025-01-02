using Microsoft.JSInterop;
using System.Text;

namespace Sparc.Kori;
public record KoriAudio(string Url, long Duration, string Voice, ICollection<KoriWord> Subtitles);
public record KoriWord(string Text, long Duration, long Offset);

public class KoriContentEngine(IKoriPages pages, IKoriContents content, IRepository<KoriTextContent> translations, KoriJsEngine js)
{
    KoriPage? CurrentPage { get; set; }
    IRepository<KoriTextContent> Translations { get; } = translations;

    public async Task InitializeAsync(Uri uri, string elementId)
    {
        await js.Init(elementId);

        var pageId = uri.GetLeftPart(UriPartial.Path);
        CurrentPage = await pages.Register(pageId, await js.GetPageTitle());
        Content = await content.ExecuteQuery("GetAll", CurrentPage.Id) ?? [];
    }

    [JSInvokable]
    public async Task<Dictionary<string, string>> RegisterAsync(Dictionary<string, string> contentNodes)
    {
        if (contentNodes.Count == 0)
            return [];

        var keysToTranslate = contentNodes
            .Where(x => !Content.ContainsKey(x.Key))
            .Select(x => x.Key)
            .Distinct()
            .ToList();

        if (keysToTranslate.Count == 0)
            return contentNodes;

        var messagesDictionary = keysToTranslate.ToDictionary(key => key, key => contentNodes[key]);
        var translatedContent = await pages.ExecuteQuery("Translate", CurrentPage.Id, messagesDictionary, request.Language);

        if (translatedContent == null)
            return contentNodes;

        foreach (var item in translatedContent)
        {
            Content[item.Value.Id] = item.Value;
        }

        foreach (var key in contentNodes.Keys.ToList())
        {
            if (Content.TryGetValue(key, out KoriTextContent? value))
            {
                contentNodes[key] = value.Text;
            }
        }

        return contentNodes;
    }

    [JSInvokable]
    public async Task<KoriTextContent> UpdateAsync(string id, string tag, string text)
    {
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

    public async Task ApplyMarkdown(string symbol, string position) => await js.ApplyMarkdown(symbol, position);

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

    public async Task Save() => await js.Save();

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
