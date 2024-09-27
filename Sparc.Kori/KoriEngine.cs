using Microsoft.JSInterop;
using Microsoft.AspNetCore.Http;

namespace Sparc.Kori;

public class KoriEngine(
    KoriLanguageEngine language,
    KoriContentEngine content,
    KoriSearchEngine search,
    KoriImageEngine images,
    KoriJsEngine js,
    KoriHttpEngine http)
{
    public static Uri BaseUri { get; set; } = new("https://localhost");
    public string Mode { get; set; } = "";

    public async Task InitializeAsync(string currentUrl)
    {
        var url = new Uri(currentUrl);
        await http.InitializeAsync(BaseUri, url.PathAndQuery, language.Value.Id);
        await content.InitializeAsync();
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

    [JSInvokable]
    public async Task<KoriTextContent> SaveAsync(string key, string text)
        => await content.SaveAsync(key, text);

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
