using Microsoft.JSInterop;
using Microsoft.AspNetCore.Http;

namespace Sparc.Kori;

public class KoriEngine(
    KoriLanguageEngine language,
    KoriHttpEngine http,
    KoriContentEngine content,
    KoriSearchEngine search,
    KoriImageEngine images,
    KoriJsEngine js)
{
    public KoriContentRequest CurrentRequest { get; private set; } = new("", "", "");
    public static Uri BaseUri { get; set; } = new("https://localhost");
    public TagManager TagManager { get; } = new TagManager();
    public string Mode { get; set; } = "";

    public async Task InitializeAsync(string currentUrl)
    {
        var url = new Uri(currentUrl);

        CurrentRequest = new KoriContentRequest(BaseUri.Host, language.Value.Id, url.PathAndQuery);

        await http.InitializeAsync(CurrentRequest);
        await content.InitializeAsync(CurrentRequest);
        await images.InitializeAsync();
    }

    public async Task InitializeAsync(HttpContext context)
    {
        await InitializeAsync(context.Request.Path);
    }

    public async Task InitializeAsync(string currentUrl, string elementId)
    {
        await InitializeAsync(currentUrl);
        await js.InvokeVoidAsync("init",
            elementId,
            language.Value.Id,
            DotNetObjectReference.Create(this),
            content.Value);
    }

    public async Task ChangeMode(string mode)
    {
        if (Mode == mode)
        {
            Mode = string.Empty;
            return;
        }

        Mode = mode;

        switch (Mode)
        {
            case "Search":
                await OpenSearchMenuAsync();
                break;
            case "Language":
                OpenTranslationMenu();
                break;
            case "Blog":
                OpenBlogMenu();
                break;
            case "A/B Testing":
                OpenABTestingMenu();
                break;
            case "Edit":
                await content.BeginEditAsync();
                break;
            case "EditImage":
                await images.BeginEditAsync();
                break;
            default:
                break;
        }
    }

    [JSInvokable]
    public async Task<Dictionary<string, string>> TranslateAsync(Dictionary<string, string> newContent)
        => await content.TranslateAsync(newContent);

    //[JSInvokable]
    //public async Task<KoriTextContent> SaveAsync(string key, string text)
    //{
    //    //TODO get tag, key and id right and check CurrentRequest
    //    return await content.SaveAsync(CurrentRequest, key, text, null, key);
    //}


    public async Task<KoriTextContent> SaveAsync(string id, string tag, string text)
        => await content.CreateOrUpdateContentAsync(CurrentRequest, id, tag, text);


    public async Task<List<KoriSearch>> SearchAsync(string searchTerm)
        => await search.SearchAsync(searchTerm);

    [JSInvokable]
    public void BackToEdit()
    {
        Mode = "";
    }

    public async Task BeginEditAsync()
    {
        var contentType = await js.InvokeAsync<string>("checkSelectedContentType");
        if (contentType == "image")
        {
            Mode = "EditImage";
            await images.BeginEditAsync();
        }
        else
        {
            Mode = "Edit";
            await content.BeginEditAsync();
        }
    }

    public async Task CloseAsync()
    {
        Mode = "Default";
        await search.CloseAsync();
    }


    public void OpenTranslationMenu()
    {
        Mode = "Language";
    }

    public async Task OpenSearchMenuAsync()
    {
        Mode = "Search";
        await search.OpenAsync();
    }

    public void OpenBlogMenu()
    {
        Mode = "Blog";
    }

    public void OpenABTestingMenu()
    {
        Mode = "ABTesting";
    }
}




// Code from Laura's branch
//using Microsoft.JSInterop;
//using System.Net.Http.Json;
//using System.Text;
//using System.Text.Json;
//using System.Net.Http.Headers;
//using Microsoft.AspNetCore.Components;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Components.Forms;

//namespace Sparc.Kori;

//public record KoriWord(string Text, long Duration, long Offset);
//public record KoriAudioContent(string Url, long Duration, string Voice, ICollection<KoriWord> Subtitles);
//public record KoriTextContent(string Id, string Tag, string Language, string Text, string Html, string ContentType, KoriAudioContent Audio, List<object>? Nodes, bool Submitted = true);
//public record SearchContentResponse(string MessageId, string MessageRoomId, string MessageRoomName, string MessageText, string MessageTag);

//public class KoriEngine(IJSRuntime js) : IAsyncDisposable
//{
//    public static Uri BaseUri { get; set; } = new("https://localhost");
//    public string RoomSlug { get; set; } = "";
//    public string Language { get; set; } = "en";
//    public string Mode { get; set; } = "";

//    Dictionary<string, KoriTextContent> _content { get; set; } = [];
//    private HttpClient Client { get; set; } = new() { BaseAddress = new Uri("https://localhost:7117/") };

//    readonly Lazy<Task<IJSObjectReference>> KoriJs = new(() => js.InvokeAsync<IJSObjectReference>("import", "./_content/Sparc.Kori/KoriTopBar.razor.js").AsTask());

//    public TagManager TagManager { get; } = new TagManager();

//    public record IbisContent(string Name, string Slug, string Language, Dictionary<string, KoriTextContent> Content);

//    public async Task InitializeAsync(HttpContext context)
//    {
//        await GetContentAsync(context.Request.Path);
//    }

//    public async Task InitializeAsync(ComponentBase component, string currentUrl, string elementId)
//    {
//        var path = new Uri(currentUrl).AbsolutePath;
//        await GetContentAsync(path);

//        var js = await KoriJs.Value;
//        await js.InvokeVoidAsync("init", elementId, Language, DotNetObjectReference.Create(component), _content);
//    }

//    public async Task<Dictionary<string, string>> TranslateAsync(Dictionary<string, string> nodes)
//    {
//        if (nodes.Count == 0)
//            return nodes;

//        var js = await KoriJs.Value;

//        var keysToTranslate = nodes.Where(x => !_content.ContainsKey(x.Key)).Select(x => x.Key).Distinct().ToList();

//        var messagesDictionary = keysToTranslate.ToDictionary(key => key, key => nodes[key]);

//        var request = new { RoomSlug, Language, Messages = messagesDictionary, AsHtml = false };

//        var content = await PostAsync<IbisContent>("publicapi/PostContent", request);

//        if (content == null)
//            return nodes;

//        foreach (var item in content.Content)
//        {
//            _content[item.Key] = item.Value;
//            _content[item.Key] = _content[item.Key] with { Nodes = new List<object>() };
//        }

//        foreach (var key in nodes.Keys.ToList())
//        {
//            if (_content.TryGetValue(key, out KoriTextContent? value))
//            {
//                nodes[key] = value.Text;
//            }
//        }

//        return nodes;
//    }

//    public async Task EditAsync()
//    {
//        var js = await KoriJs.Value;

//        var contentType = await js.InvokeAsync<string>("checkSelectedContentType");

//        if (contentType == "image")
//        {
//            Mode = "EditImage";
//            await js.InvokeVoidAsync("editImage");
//        }
//        else
//        {
//            Mode = "Edit";
//            await js.InvokeVoidAsync("edit");
//        }
//    }

//    public async Task CancelAsync()
//    {
//        var js = await KoriJs.Value;
//        await js.InvokeVoidAsync("cancelEdit");
//    }

//    public async Task CloseAsync()
//    {
//        Mode = "Default";
//        var js = await KoriJs.Value;
//        await js.InvokeVoidAsync("closeSearch");
//        Console.WriteLine("called the JS function");
//    }

//    public async Task BeginSaveAsync()
//    {
//        var js = await KoriJs.Value;

//        var contentType = await js.InvokeAsync<string>("checkSelectedContentType");

//        if (contentType == "image" && selectedImage != null)
//        {
//            var originalImageSrc = await GetActiveImageSrcFromJs();

//            if (originalImageSrc != null)
//            {
//                await SaveImageAsync(originalImageSrc, selectedImage);
//            }
//        }
//        else
//        {

//            await js.InvokeVoidAsync("save");
//        }
//    }

//    public async Task<KoriTextContent> SaveAsync(string key, string text)
//    {
//        var request = new { RoomSlug, Language, Tag = key, Text = text };
//        var result = await PostAsync<KoriTextContent>("publicapi/TypeMessage", request);
//        return result!;
//    }

//    public async Task<string> GetActiveImageSrcFromJs()
//    {
//        var js = await KoriJs.Value;
//        return await js.InvokeAsync<string>("getActiveImageSrc");
//    }

//    private IBrowserFile? selectedImage;

//    public void OnImageSelected(InputFileChangeEventArgs e)
//    {
//        selectedImage = e.File;
//    }

//    private async Task SaveImageAsync(string key, IBrowserFile imageFile)
//    {
//        using var content = new MultipartFormDataContent();

//        var size15MB = 1024 * 1024 * 15;
//        var fileContent = new StreamContent(imageFile.OpenReadStream(maxAllowedSize: size15MB));
//        fileContent.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType);
//        content.Add(fileContent, "File", imageFile.Name);

//        content.Add(new StringContent(RoomSlug), "RoomSlug");
//        content.Add(new StringContent(Language), "Language");
//        content.Add(new StringContent(key), "Tag");

//        var response = await Client.PostAsync("publicapi/UploadImage", content);

//        var result = await response.Content.ReadAsStringAsync();
//        var savedImg = JsonSerializer.Deserialize<KoriTextContent>(result, JsonOptions);

//        if (response.IsSuccessStatusCode && savedImg != null)
//        {
//            var js = await KoriJs.Value;
//            await js.InvokeVoidAsync("updateImageSrc", key, savedImg.Text);
//            Console.WriteLine("Image sent successfully!");
//        }
//        else
//        {
//            Console.WriteLine("Error sending image: " + response.StatusCode);
//        }
//    }

//    public async Task PlayAsync(KoriTextContent content)
//    {
//        if (content?.Audio?.Url == null)
//            return;

//        var js = await KoriJs.Value;
//        await js.InvokeVoidAsync("playAudio", content.Audio.Url);
//    }

//    public async ValueTask DisposeAsync()
//    {
//        if (KoriJs.IsValueCreated)
//        {
//            var module = await KoriJs.Value;
//            await module.DisposeAsync();
//        }
//    }

//    static string LoremIpsum(int wordCount)
//    {
//        var words = new[]{"lorem", "ipsum", "dolor", "sit", "amet", "consectetuer",
//        "adipiscing", "elit", "sed", "diam", "nonummy", "nibh", "euismod",
//        "tincidunt", "ut", "laoreet", "dolore", "magna", "aliquam", "erat"};

//        var rand = new Random();
//        StringBuilder result = new();

//        for (int i = 0; i < wordCount; i++)
//        {
//            var word = words[rand.Next(words.Length)];
//            var punctuation = i == wordCount - 1 ? "." : rand.Next(8) == 2 ? "," : "";

//            if (i > 0)
//                result.Append($" {word}{punctuation}");
//            else
//                result.Append($"{word[0].ToString().ToUpper()}{word.AsSpan(1)}");
//        }

//        return result.ToString();
//    }

//    private async Task GetContentAsync(string path)
//    {
//        RoomSlug = $"{BaseUri.Host}{path}";

//        var request = new
//        {
//            RoomSlug,
//            Language
//        };

//        var content = await PostAsync<IbisContent>("publicapi/PostContent", request);
//        if (content != null)
//            _content = content.Content.ToDictionary(x => x.Key, x => x.Value with { Nodes = [] });
//    }

//    public async Task<List<SearchContentResponse>> SearchContentAsync(string searchTerm)
//    {
//        var request = new { SearchTerm = searchTerm };
//        var result = await PostAsync<List<SearchContentResponse>>("publicapi/SearchContent", request);
//        return result;
//    }

//    private static JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
//    async Task<TResponse?> PostAsync<TResponse>(string url, object request)
//    {
//        try
//        {
//            var response = await Client.PostAsJsonAsync(url, request);
//            var result = await response.Content.ReadAsStringAsync();
//            return JsonSerializer.Deserialize<TResponse>(result, JsonOptions);
//        }
//        catch (Exception)
//        {
//            return default;
//        }
//    }

//    public void OpenTranslationMenu()
//    {
//        Mode = "Language";
//    }

//    public async Task OpenSearchMenuAsync()
//    {
//        var js = await KoriJs.Value;
//        Mode = "Search";
//        await js.InvokeVoidAsync("showSidebar");
//    }

//    public void OpenBlogMenu()
//    {
//        Mode = "Blog";
//    }

//    public void OpenABTestingMenu()
//    {
//        Mode = "ABTesting";
//    }


//    public async Task ApplyMarkdown(string symbol, string position)
//    {
//        Console.WriteLine("Apply Markdown");
//        var js = await KoriJs.Value;
//        await js.InvokeVoidAsync("applyMarkdown", symbol, position);
//    }

//    public void BackToEdit()
//    {
//        Mode = "";
//    }
//}

public class TagManager
{
    private readonly Dictionary<string, string> dict = new Dictionary<string, string>();

    public string this[string key]
    {
        get
        {
            if (!dict.ContainsKey(key))
            {
                dict.Add(key, "");
            }
            return dict[key];
        }
        set
        {
            dict[key] = value;
        }
    }
}