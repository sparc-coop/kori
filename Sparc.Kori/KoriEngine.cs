using Microsoft.JSInterop;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

namespace Sparc.Kori;

public class KoriEngine(
    KoriLanguageEngine language,
    KoriContentEngine content,
    KoriSearchEngine search,
    KoriImageEngine images,
    KoriJsEngine js)
{
    public static Uri BaseUri { get; set; } = new("https://localhost");
    public string CurrentUrl { get; set; } = "";
    public string Mode { get; set; } = "";

    public async Task InitializeAsync(HttpContext context)
    {
        CurrentUrl = $"{BaseUri.Host}{context.Request.Path}";
        await content.InitializeAsync(CurrentUrl, language.Value);
        await images.InitializeAsync(CurrentUrl, language.Value);
    }

    public async Task InitializeAsync(ComponentBase component, string currentUrl, string elementId)
    {
        CurrentUrl = new Uri(currentUrl).AbsolutePath;
        await content.InitializeAsync(CurrentUrl, language.Value);
        await images.InitializeAsync(CurrentUrl, language.Value);
        await js.InvokeVoidAsync("init", 
            elementId, 
            language.Value, 
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
