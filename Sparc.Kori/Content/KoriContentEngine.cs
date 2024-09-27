namespace Sparc.Kori;

public record KoriContentRequest(string Path, string Language);
public record KoriPage(string Name, string Slug, string Language, ICollection<KoriTextContent> Content);
public record KoriTextContent(string Id, string Tag, string Language, string Text, string Html, string ContentType, KoriAudioContent Audio, List<object>? Nodes, bool Submitted = true);
public record KoriAudioContent(string Url, long Duration, string Voice, ICollection<KoriWord> Subtitles);
public record KoriWord(string Text, long Duration, long Offset);

public class KoriContentEngine(HttpClient client, KoriJsEngine js)
{
    public Dictionary<string, KoriTextContent> Value { get; set; } = [];
    public string EditMode { get; set; } = "Edit";
    KoriContentRequest? CurrentRequest;

    public async Task InitializeAsync(string path, string language)
    {
        CurrentRequest = new KoriContentRequest(path, language);

        var content = await client.PostAsync<KoriPage>("publicapi/PostContent", CurrentRequest);
        if (content != null)
            Value = content.Content.ToDictionary(x => x.Tag, x => x with { Nodes = [] });
    }

    public async Task<Dictionary<string, string>> TranslateAsync(Dictionary<string, string> nodes)
    {
        if (nodes.Count == 0 || CurrentRequest == null)
            return nodes;

        var keysToTranslate = nodes.Where(x => !Value.ContainsKey(x.Key)).Select(x => x.Key).Distinct().ToList();
        var messagesDictionary = keysToTranslate.ToDictionary(key => key, key => nodes[key]);
        var request = new { CurrentRequest.Path, CurrentRequest.Language, Messages = messagesDictionary, AsHtml = false };
        var content = await client.PostAsync<KoriPage>("publicapi/PostContent", request);

        if (content == null)
            return nodes;

        foreach (var item in content.Content)
        {
            Value[item.Tag] = item with { Nodes = new List<object>() };
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

    public async Task CancelAsync()
    {
        await js.InvokeVoidAsync("cancelEdit");
    }

    public async Task<KoriTextContent> SaveAsync(string key, string text)
    {
        if (CurrentRequest == null)
            throw new InvalidOperationException("Content not initialized");

        var request = new { CurrentRequest.Path, CurrentRequest.Language, Tag = key, Text = text };
        var result = await client.PostAsync<KoriTextContent>("publicapi/TypeMessage", request);
        return result!;
    }


}
