namespace Kori;

public interface ITranslator
{
    Task<List<Content>> TranslateAsync(Content message, List<Language> toLanguages);
    Task<List<Language>> GetLanguagesAsync();
    async Task<Language?> GetLanguageAsync(string language)
    {
        var languages = await GetLanguagesAsync();
        return languages.FirstOrDefault(x => x.Id == language);
    }
}
