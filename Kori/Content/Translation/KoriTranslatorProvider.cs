using DeepL;

namespace Kori;

public class KoriTranslatorProvider
{
    internal static List<Language>? Languages;

    public KoriTranslatorProvider(IEnumerable<ITranslator> translators, IRepository<Content> content)
    {
        Translators = translators;
        Content = content;
    }

    internal IEnumerable<ITranslator> Translators { get; }
    public IRepository<Content> Content { get; }

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

    //internal async Task<List<Content>> TranslateAsync(Content message, string toLanguage)
    //{
    //    var language = await GetLanguageAsync(toLanguage)
    //        ?? throw new ArgumentException($"Language {toLanguage} not found");
    //    return await TranslateAsync(message, [language]);
    //}

    //internal async Task<List<Content>> TranslateAsync(IEnumerable<Content> messages, List<Language> toLanguages)
    //{
    //    var processedLanguages = new List<Language>(toLanguages);
    //    var messages = new List<Content>();
    //    foreach (var translator in Translators)
    //    {
    //        var languages = await translator.GetLanguagesAsync();
    //        if (!languages.Any(x => x.Id.ToUpper() == message.Language.ToUpper()))
    //            continue;

    //        try
    //        {
    //            var languagesToTranslate = processedLanguages.Where(x => languages.Any(y => y.Id.ToUpper() == x.Id.ToUpper())).ToList();
    //            messages.AddRange(await translator.TranslateAsync(message, languagesToTranslate));
    //            processedLanguages.RemoveAll(x => languagesToTranslate.Any(y => y.Id.ToUpper() == x.Id.ToUpper()));
    //            if (!processedLanguages.Any())
    //                break;
    //        }
    //        catch
    //        {
    //            continue;
    //        }
    //    }

    //    return messages;
    //}

    //internal async Task<string?> TranslateAsync(string text, string fromLanguage, string toLanguage)
    //{
    //    if (fromLanguage == toLanguage)
    //        return text;

    //    var language = await GetLanguageAsync(toLanguage)
    //        ?? throw new ArgumentException($"Language {toLanguage} not found");

    //    var message = new Content("", fromLanguage, text);
    //    var result = await TranslateAsync([message], [language]);
    //    return result?.FirstOrDefault()?.Text;
    //}

    public async Task<ITranslator?> For(string fromLanguage, string toLanguage)
    {
        foreach (var translator in Translators)
        {
            if (await translator.CanTranslateAsync(fromLanguage, toLanguage))
                return translator;
        }

        return null;
    }

    public async Task<ITranslator?> For(Content originalContent, string toLanguage)
        => await For(originalContent.Language, toLanguage);
}
