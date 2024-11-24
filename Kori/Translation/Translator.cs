namespace Kori;

internal class Translator
{
    internal static List<Language>? Languages;

    public Translator(IEnumerable<ITranslator> translators)
    {
        Translators = translators;
    }

    internal IEnumerable<ITranslator> Translators { get; }

    internal async Task<List<Language>> GetLanguagesAsync()
    {
        if (Languages == null)
        {
            Languages = [];
            foreach (var translator in Translators)
            {
                var languages = await translator.GetLanguagesAsync();
                Languages.AddRange(languages.Where(x => !Languages.Any(y => y.Id.ToUpper() == x.Id.ToUpper())));
            }
        }

        return Languages;
    }

    internal async Task<Language?> GetLanguageAsync(string language)
    {
        var languages = await GetLanguagesAsync();
        return languages.FirstOrDefault(x => x.Id == language);
    }

    internal async Task<List<Content>> TranslateAsync(Content message, List<Language> toLanguages)
    {
        var processedLanguages = new List<Language>(toLanguages);
        var messages = new List<Content>();
        foreach (var translator in Translators)
        {
            var languages = await translator.GetLanguagesAsync();
            if (!languages.Any(x => x.Id.ToUpper() == message.Language.ToUpper()))
                continue;

            try
            {
                var languagesToTranslate = processedLanguages.Where(x => languages.Any(y => y.Id.ToUpper() == x.Id.ToUpper())).ToList();
                messages.AddRange(await translator.TranslateAsync(message, languagesToTranslate));
                processedLanguages.RemoveAll(x => languagesToTranslate.Any(y => y.Id.ToUpper() == x.Id.ToUpper()));
                if (!processedLanguages.Any())
                    break;
            }
            catch
            {
                continue;
            }
        }

        return messages;
    }

    internal async Task<string?> TranslateAsync(string text, string fromLanguage, string toLanguage)
    {
        if (fromLanguage == toLanguage)
            return text;

        var language = await GetLanguageAsync(toLanguage)
            ?? throw new ArgumentException($"Language {toLanguage} not found");

        var message = new Content("", "", fromLanguage, text);
        var result = await TranslateAsync(message, [language]);
        return result?.FirstOrDefault()?.Text;
    }

}
