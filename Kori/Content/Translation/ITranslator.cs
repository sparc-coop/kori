namespace Kori;

public interface ITranslator
{
    Task<List<Content>> TranslateAsync(IEnumerable<Content> messages, IEnumerable<Language> toLanguages);
    Task<List<Language>> GetLanguagesAsync();
    async Task<Language?> GetLanguageAsync(string language)
    {
        var languages = await GetLanguagesAsync();
        return languages.FirstOrDefault(x => x.Id == language);
    }

    Task<Content?> TranslateAsync(Content message, Language toLanguage);

    async Task<Content?> TranslateAsync(Content message, string toLanguage)
    {
        var language = await GetLanguageAsync(toLanguage)
            ?? throw new ArgumentException($"Language {toLanguage} not found");
        return await TranslateAsync(message, language);
    }

    async Task<List<Content>> TranslateAsync(IEnumerable<Content> messages, string toLanguage)
    {
        var language = await GetLanguageAsync(toLanguage)
            ?? throw new ArgumentException($"Language {toLanguage} not found");
        return await TranslateAsync(messages, [language]);
    }

    async Task<string?> TranslateAsync(string text, string fromLanguage, string toLanguage)
    {
        var language = await GetLanguageAsync(toLanguage)
            ?? throw new ArgumentException($"Language {toLanguage} not found");

        var message = new Content("", text, fromLanguage);
        var result = await TranslateAsync([message], [language]);
        return result?.FirstOrDefault()?.Text;
    }

    async Task<bool> CanTranslateAsync(string fromLanguage, string toLanguage)
    {
        var from = await GetLanguageAsync(fromLanguage);
        var to = await GetLanguageAsync(toLanguage);
        return from != null && to != null;
    }
}