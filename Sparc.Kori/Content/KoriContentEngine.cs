using System.Text;

namespace Sparc.Kori;

public record KoriPage(string Name, string Slug, string Language, ICollection<KoriTextContent> Content);
public record KoriTextContent(string Id, string Tag, string Language, string Text, string Html, string ContentType, KoriAudioContent Audio, List<object>? Nodes, bool Submitted = true);
public record KoriAudioContent(string Url, long Duration, string Voice, ICollection<KoriWord> Subtitles);
public record KoriWord(string Text, long Duration, long Offset);

public class KoriContentEngine(KoriHttpEngine http, KoriJsEngine js)
{
    public Dictionary<string, KoriTextContent> Value { get; set; } = [];
    public string EditMode { get; set; } = "Edit";

    public async Task InitializeAsync()
    {
        Value = await http.GetContentAsync() ?? [];
    }

    public async Task<Dictionary<string, string>> TranslateAsync(Dictionary<string, string> nodes)
    {
        if (nodes.Count == 0)
            return nodes;

        var keysToTranslate = nodes.Where(x => !Value.ContainsKey(x.Key)).Select(x => x.Key).Distinct().ToList();
        var messagesDictionary = keysToTranslate.ToDictionary(key => key, key => nodes[key]);
        var content = await http.TranslateAsync(messagesDictionary);
        if (content == null)
            return nodes;

        foreach (var item in content)
        {
            Value[item.Tag] = item with { Nodes = [] };
        }

        foreach (var key in nodes.Keys.ToList())
        {
            if (Value.TryGetValue(key, out KoriTextContent? value))
            {
                nodes[key] = value.Text;
            }
        }

        return nodes;
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

    public async Task<KoriTextContent> SaveAsync(string key, string text)
        => await http.SaveContentAsync(key, text);

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
