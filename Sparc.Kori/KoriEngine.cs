using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
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

    public async Task InitializeAsync(ComponentBase component, string currentUrl, string elementId)
    {
        await InitializeAsync(currentUrl);
        await js.InvokeVoidAsync("init", 
            elementId, 
            language.Value.Id, 
            DotNetObjectReference.Create(component), 
            content.Value);
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


    public void BackToEdit()
    {
        Mode = "";
    }
}
