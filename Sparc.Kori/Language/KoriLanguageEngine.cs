using System.Globalization;
using System.Net.Http.Json;

namespace Sparc.Kori;

public record KoriLanguage(string Id, string DisplayName, string NativeName, bool IsRightToLeft);
public class KoriLanguageEngine(HttpClient client)
{
    public List<KoriLanguage> AllLanguages { get; set; } = [];
    public KoriLanguage Value { get; set; } = new("en", "English", "English", false);
    public HttpClient Client { get; } = client;

    public async Task<List<KoriLanguage>> InitializeAsync(KoriLanguage? selectedLanguage)
    {
        if (AllLanguages.Count == 0)
            AllLanguages = await Client.GetFromJsonAsync<List<KoriLanguage>>("api/Languages")
            ?? [];

        AllLanguages = AllLanguages.OrderBy(x => x.DisplayName).ToList();

        var selectedLanguageId = selectedLanguage?.Id ?? CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        Value = AllLanguages.FirstOrDefault(x => x.Id == selectedLanguageId) ?? Value;
        
        return AllLanguages;
    }
}

