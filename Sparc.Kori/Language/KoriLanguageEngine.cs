using System.Net.Http.Json;

namespace Sparc.Kori;

public record KoriLanguage(string Id, string DisplayName, string NativeName, bool IsRightToLeft);
public class KoriLanguageEngine(HttpClient client)
{
    public string Value { get; set; } = "en";
    public HttpClient Client { get; } = client;

    public async Task<List<KoriLanguage>> GetAllAsync()
    {
        return await Client.GetFromJsonAsync<List<KoriLanguage>>("api/Languages")
            ?? [];
    }
}

