
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Sparc.Kori;

public record KoriContentRequest(string Domain, string Language, string Path);

public class KoriHttpEngine(HttpClient client)
{
    private KoriContentRequest CurrentRequest = new("", "", "");

    public Task InitializeAsync(KoriContentRequest request)
    {
        CurrentRequest = request;
        return Task.CompletedTask;
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

    internal async Task<KoriPage> GetPageByDomainAndPathAsync(string domain, string? path = null)
    {
        var requestUri = $"pages/GetByDomainAndPath?domain={domain}";

        if (!string.IsNullOrEmpty(path)) requestUri = $"{requestUri}&path={path}";

        var result = await client.GetFromJsonAsync<List<KoriPage>>(requestUri);

        return result.FirstOrDefault();
    }

    internal async Task<KoriPage> CreatePage(string domain, string path, string name)
    {
        var requestUri = $"pages?domain={domain}";

        requestUri = appendQueryParameter(requestUri, "path", path);
        requestUri = appendQueryParameter(requestUri, "name", name);

        var result = await client.PostAsync<KoriPage>(requestUri, new());

        return result;
    }

    private static string appendQueryParameter(string requestUri, string name, string value)
    {
        if (!string.IsNullOrEmpty(value)) requestUri = $"{requestUri}&{name}={value}";
        return requestUri;
    }

    public async Task<Dictionary<string, KoriTextContent>?> GetContentAsync(string domain, string? path = null)
    {
        var requestUri = $"contents/GetByDomainAndPath?domain={domain}";

        if (!string.IsNullOrEmpty(path)) requestUri = $"{requestUri}&path={path}";

        var result = await client.GetFromJsonAsync<ICollection<KoriTextContent>>(requestUri);

        return result?.ToDictionary(x => x.Tag, x => x with { Nodes = [] });
    }    

    internal async Task<KoriTextContent> GetContentByIdAsync(string id)
    {
        var result = await client.GetFromJsonAsync<KoriTextContent>($"contents/{id}");
        return result;
    }

    internal async Task<KoriTextContent> CreateContent(string domain, string path, string language, string? tag, string text, string contentType)
    {
        tag ??= text;
        var request = new { domain, path, language, tag, text, contentType };
        var result = await client.PostAsync<KoriTextContent>($"contents", request);
        return result;
    }

    internal async Task<KoriTextContent> UpdateTextContentAsync(string id, string tag, string text)
    {
        var request = new { id, tag, text };
        var response = await client.PutAsJsonAsync($"contents/{id}/SetText", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<KoriTextContent>();
        return result!;
    }
}
