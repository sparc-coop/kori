
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Sparc.Kori;

public record KoriContentRequest(string Domain, string Language, string Path);
public class KoriHttpEngine(HttpClient client)
{
    KoriContentRequest CurrentRequest = new("", "", "");

    public Task InitializeAsync(Uri baseUri, string path, string language)
    {
        CurrentRequest = new(baseUri.Host, language, path);
        return Task.CompletedTask;
    }

    public async Task<Dictionary<string, KoriTextContent>?> GetContentAsync()
    {
        var content = await client.PostAsync<KoriPage>("api/Content", CurrentRequest);
        return content?.Content.ToDictionary(x => x.Tag, x => x with { Nodes = [] });
    }

    internal async Task<ICollection<KoriTextContent>?> TranslateAsync(Dictionary<string, string> messagesDictionary)
    {
        var request = new { CurrentRequest.Path, CurrentRequest.Language, Messages = messagesDictionary, AsHtml = false };
        var result = await client.PostAsync<KoriPage>($"api/Content", request);
        return result?.Content;
    }

    public async Task<KoriTextContent> SaveContentAsync(string key, string text)
    {
        var request = new { CurrentRequest.Path, CurrentRequest.Language, Tag = key, Text = text };
        var result = await client.PostAsync<KoriTextContent>("api/Content", request);
        return result!;
    }

    public async Task<KoriTextContent> SaveContentAsync(string key, IBrowserFile image)
    {
        using var content = new MultipartFormDataContent();

        var size15MB = 1024 * 1024 * 15;
        var fileContent = new StreamContent(image.OpenReadStream(maxAllowedSize: size15MB));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);
        content.Add(fileContent, "File", image.Name);
        content.Add(new StringContent(CurrentRequest.Domain), "Domain");
        content.Add(new StringContent(CurrentRequest.Path), "Path");
        content.Add(new StringContent(CurrentRequest.Language), "Language");
        content.Add(new StringContent(key), "Key");

        var response = await client.PostAsync<KoriTextContent>("api/Images", content);
        return response!;
    }

    internal async Task<List<KoriLanguage>> GetLanguagesAsync()
    {
       var result = await client.GetFromJsonAsync<List<KoriLanguage>>("api/Languages");
       return result;
    }

    internal async Task<List<KoriSearch>> SearchContentAsync(string searchTerm)
    {
        var result = await client.GetFromJsonAsync<List<KoriSearch>>($"contents/Search?searchTerm={searchTerm}");
        return result.Select(item => item with { Source = "content" }).ToList();
    }

    internal async Task<List<KoriSearch>> SearchPageAsync(string searchTerm)
    {
        var result = await client.GetFromJsonAsync<List<KoriSearch>>($"pages/Search?searchTerm={searchTerm}");
        return result.Select(item => item with { Source = "page" }).ToList();
    }
}
