namespace Kori;

public interface ITranslator
{
    int Priority { get; }
    
    Task<List<Content>> TranslateAsync(IEnumerable<Content> messages, IEnumerable<Language> toLanguages);
    Task<List<Language>> GetLanguagesAsync();
    async Task<Language?> GetLanguageAsync(string language)
    {
        var languages = await GetLanguagesAsync();
        return languages.FirstOrDefault(x => x.Id == language);
    }

    async Task<Language?> GetLanguageAsync(Language language) => await GetLanguageAsync(language.Id);

    Task<Content?> TranslateAsync(Content message, Language toLanguage);

    async Task<List<Content>> TranslateAsync(IEnumerable<Content> messages, Language toLanguage)
    {
        var language = await GetLanguageAsync(toLanguage)
            ?? throw new ArgumentException($"Language {toLanguage} not found");
        return await TranslateAsync(messages, [language]);
    }

    async Task<string?> TranslateAsync(string text, Language fromLanguage, Language toLanguage)
    {
        var language = await GetLanguageAsync(toLanguage)
            ?? throw new ArgumentException($"Language {toLanguage} not found");

        var message = new Content("", fromLanguage, text);
        var result = await TranslateAsync([message], [language]);
        return result?.FirstOrDefault()?.Text;
    }

    async Task<bool> CanTranslateAsync(Language fromLanguage, Language toLanguage)
    {
        var from = await GetLanguageAsync(fromLanguage);
        var to = await GetLanguageAsync(toLanguage);
        return from != null && to != null;
    }
}